using System;
using System.Linq;
using System.Text;

namespace RubyStack
{
	public class ProcessResult
	{
		public string RawOutput {
			get {
				return string.Join (Environment.NewLine, processOutput.Select (x => x.Data));
			}
		}

		readonly CommandOutput[] processOutput;

		public CommandOutput[] Output {
			get {
				return processOutput;
			}
		}

		public int ExitCode { get; private set; }

		public long ElapsedMilliseconds { get; private set; }

		public bool HasExited { get; private set; }

		public ProcessResult (CommandOutput[] processOutput, int exitCode, long elapsedMilliseconds, bool hasExited)
		{
			this.processOutput = processOutput;
			ExitCode = exitCode;
			ElapsedMilliseconds = elapsedMilliseconds;
			HasExited = hasExited;
		}

		public override string ToString ()
		{
			var stringBuilder = new StringBuilder ();

			var message = HasExited ?
				$"Finished with exit code {ExitCode} in {ElapsedMilliseconds} ms.{Environment.NewLine}" :
				$"Process output after {ElapsedMilliseconds} ms (still running).{Environment.NewLine}";

			stringBuilder.Append (message);

			foreach (var output in processOutput ?? new CommandOutput[0]) {
				if (output != null)
					stringBuilder.AppendLine (output.Data);
			}

			return stringBuilder.ToString ();
		}
	}
}
