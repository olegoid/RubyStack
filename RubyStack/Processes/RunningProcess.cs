using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RubyStack
{
	public class RunningProcess
	{
		internal class MaxCapacityQueue<T> : IEnumerable<T>
		{
			public int Capacity { get; private set; }

			public Queue<T> Queue { get; private set; }

			public MaxCapacityQueue (int capacity) 
			{
				Capacity = capacity;
				Queue = new Queue<T> ();
			}

			public void Enqueue (T item) 
			{
				Queue.Enqueue (item);
				if (Capacity != -1 && Queue.Count > Capacity)
					Queue.Dequeue ();
			}

			IEnumerator<T> IEnumerable<T>.GetEnumerator ()
			{
				return Queue.GetEnumerator ();
			}

			public IEnumerator GetEnumerator ()
			{
				return Queue.GetEnumerator ();
			}

			public T[] ToArray () 
			{
				return Queue.ToArray ();
			}

			public void Clear ()
			{
				Queue.Clear ();
			}
		}

		readonly Process process;
		readonly MaxCapacityQueue<CommandOutput> processOutput;
		readonly object processsOutputLock = new object ();
		readonly Stopwatch stopwatch;

		public RunningProcess (string path, string arguments, Predicate<string> dropFilter = null, int maxNumberOfLines = -1)
		{
			stopwatch = Stopwatch.StartNew ();

			processOutput = new MaxCapacityQueue<CommandOutput> (maxNumberOfLines);

			var psi = new ProcessStartInfo ();

			psi.FileName = path;
			psi.Arguments = arguments;

			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardError = true;
			psi.CreateNoWindow = true;

			process = new Process {
				StartInfo = psi,
				EnableRaisingEvents = true
			};

			process.OutputDataReceived += (sender, args) => {
				if (args.Data == null || (dropFilter != null && dropFilter (args.Data)))
					return;

				lock (processsOutputLock) {
					processOutput.Enqueue (new CommandOutput (args.Data, stopwatch.ElapsedMilliseconds));
				}
			};

			process.ErrorDataReceived += (sender, args) => {
				if (args.Data == null)
					return;

				lock (processsOutputLock) {
					processOutput.Enqueue (new CommandOutput (args.Data, stopwatch.ElapsedMilliseconds));
				}
			};

			process.Start ();
			process.BeginOutputReadLine ();
			process.BeginErrorReadLine ();

			process.Exited += (sender, e) => {
				stopwatch.Stop ();
			};
		}

		public ProcessResult WaitForExit ()
		{
			process.WaitForExit ();
			return GetOutput ();
		}

		public void Kill ()
		{
			process.Kill ();
		}

		public ProcessResult GetOutput (bool removeEmptyLines = false)
		{
			var exitCode = process.HasExited ? process.ExitCode : 0;

			lock (processsOutputLock) {
				if (removeEmptyLines) {
					return new ProcessResult (processOutput.Where (x => !string.IsNullOrEmpty (x.Data)).ToArray (), 
						exitCode, stopwatch.ElapsedMilliseconds, process.HasExited);
				}

				return new ProcessResult (processOutput.ToArray (), exitCode, stopwatch.ElapsedMilliseconds,
					process.HasExited);
			}
		}

		public ProcessResult GetOutputAndFlush (bool removeEmptyLines = false)
		{
			var exitCode = process.HasExited ? process.ExitCode : 0;

			lock (processsOutputLock) {
				var processOutputArray = removeEmptyLines
					? processOutput.Where (x => !string.IsNullOrEmpty (x.Data)).ToArray ()
					: processOutput.ToArray ();

				processOutput.Clear ();
				return new ProcessResult (processOutputArray, exitCode, stopwatch.ElapsedMilliseconds, process.HasExited);
			}
		}
	}
}
