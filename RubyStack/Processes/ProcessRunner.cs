using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RubyStack
{
	public class ProcessRunner : IProcessRunner
	{
		public ProcessResult Run (string path, string arguments = null, IEnumerable<int> noExceptionOnExitCodes = null)
		{
			var output = RunProcessWaitForExit (path, arguments);

			if (ExitCodeIsUnexpected (output.ExitCode, noExceptionOnExitCodes)) {
				var message = $"Failed to execute: {path} {arguments} - exit code: {output.ExitCode}{Environment.NewLine}{output.Output}";
				throw new Exception (message);
			}

			return output;
		}

		public ProcessResult RunCommand (string command, string arguments = null, CheckExitCode checkExitCode = CheckExitCode.FailIfNotSuccess)
		{
			var output = RunProcessWaitForExit (command, arguments);

			if (checkExitCode == CheckExitCode.FailIfNotSuccess && output.ExitCode != 0)
				throw new Exception ($"Failed to execute: {command} {arguments} - exit code: {output.ExitCode}{Environment.NewLine}{output.Output}");

			return output;
		}

		bool ExitCodeIsUnexpected (int exitCode, IEnumerable<int> noExceptionOnExitCodes = null)
		{
			return exitCode != 0 && (!noExceptionOnExitCodes?.Any (e => e.Equals (exitCode)) ?? true);
		}

		static ProcessResult RunProcessWaitForExit (string path, string arguments = null)
		{
			var processOutputArray = new List<CommandOutput> ();
			var startInfo = new ProcessStartInfo (path, arguments) {
				RedirectStandardOutput = true,
				UseShellExecute = false
			};

			var stopwatch = Stopwatch.StartNew ();
			var process = new Process ();

			try {
				process.StartInfo = startInfo;
				process.OutputDataReceived += (sender, e) => {
					processOutputArray.Add (new CommandOutput (e.Data, stopwatch.ElapsedMilliseconds));
				};

				process.Start ();
				process.WaitForExit ();
			} catch (Win32Exception) {
				processOutputArray.Clear ();
			}

			return new ProcessResult (processOutputArray.ToArray (), process.ExitCode, stopwatch.ElapsedMilliseconds, true);
		}

		static async Task ReadOutput (StreamReader reader, List<CommandOutput> processOutput, object processOutputLock, Stopwatch stopwatch)
		{
			string line;
			while ((line = await reader.ReadLineAsync ()) != null) {
				lock (processOutputLock)
					processOutput.Add (new CommandOutput (line, stopwatch.ElapsedMilliseconds));
			}
		}
		
		public ProcessResult Run (string path, string arguments = null, IEnumerable<string> stdinCommands = null, IEnumerable<int> noExceptionOnExitCodes = null)
		{
			var processOutputArray = new List<CommandOutput> ();
			var startInfo = new ProcessStartInfo(path, arguments) {
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				RedirectStandardInput = true,
				UseShellExecute = false
			};

			var stopwatch = Stopwatch.StartNew ();
			var process = new Process ();

			try {
				process.StartInfo = startInfo;

				process.Start ();

				process.OutputDataReceived += (sender, e) => {
					processOutputArray.Add (new CommandOutput (e.Data, stopwatch.ElapsedMilliseconds));
				};

				process.ErrorDataReceived += (sender, e) => {
					processOutputArray.Add (new CommandOutput (e.Data, stopwatch.ElapsedMilliseconds));
				};

				process.BeginErrorReadLine ();
				process.BeginOutputReadLine ();

				if (stdinCommands != null) {
					using (StreamWriter sw = process.StandardInput) {
						if (sw.BaseStream.CanWrite) 
							foreach (var stdinCommand in stdinCommands)
								sw.WriteLine (stdinCommand);
					}
				}

				process.WaitForExit ();
			} catch (Win32Exception) {
				processOutputArray.Clear ();
			}

			return new ProcessResult (processOutputArray.ToArray (), process.ExitCode, stopwatch.ElapsedMilliseconds, true);
		}
	}
}
