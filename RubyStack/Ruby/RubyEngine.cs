using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RubyStack
{
	public class RubyEngine
	{
		readonly List<Tuple<IReturn, string, Action<IRubyExpressionResult>>> commandsQueue;

		Process irbProcess;
		IReturn activeCommand;

		public RubyEngine (string pathToExecEngine = "")
		{
			if (string.IsNullOrEmpty (pathToExecEngine))
				pathToExecEngine = "/usr/bin/irb";

			commandsQueue = new List<Tuple<IReturn, string, Action<IRubyExpressionResult>>> ();
			StartEngine (pathToExecEngine);
		}

		public async Task<IRubyExpressionResult> Run (IReturn expression)
		{
			try {
				var taskCompletionSource = new TaskCompletionSource<IRubyExpressionResult> ();
				var task = taskCompletionSource.Task;
				Action<IRubyExpressionResult> callback = taskCompletionSource.SetResult;

				await Task.Factory.StartNew (() => {
					try {
						Run (expression, callback);
					} catch (Exception exception) {
						taskCompletionSource.SetException (exception);
					}
				}, TaskCreationOptions.AttachedToParent);

				return await Task.FromResult (task.Result);
			} catch {
				return null;
			}
		}

		void Run (IReturn expression, Action<IRubyExpressionResult> callback)
		{
			var command = CommandBuilder.BuildCommandForExpression (expression);
			commandsQueue.Add (new Tuple<IReturn, string, Action<IRubyExpressionResult>> (expression, command, callback));

			using (StreamWriter sw = irbProcess.StandardInput) {
				if (sw.BaseStream.CanWrite)
					sw.WriteLine (command);
			}
		}

		void StartEngine (string pathToExecEngine)
		{
			var startInfo = new ProcessStartInfo (pathToExecEngine) {
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				RedirectStandardInput = true,
				UseShellExecute = false
			};

			irbProcess = new Process {
				StartInfo = startInfo
			};

			irbProcess.Start ();
			irbProcess.OutputDataReceived += OnOutputDataReceived;

			irbProcess.BeginErrorReadLine ();
			irbProcess.BeginOutputReadLine ();
		}

		public void Terminate ()
		{
			irbProcess.Kill ();
		}

		void OnOutputDataReceived (object sender, DataReceivedEventArgs e)
		{
			var command = commandsQueue.FirstOrDefault (c => c.Item2 == e.Data);
			var expr = command?.Item1;

			if (expr != null) {
				activeCommand = expr;
				return;
			}

			if (activeCommand == null)
				return;

			var commandInQueue = commandsQueue.FirstOrDefault (t => t.Item1 == activeCommand);
			var returnable = activeCommand.GetType ().GetTypeWithGenericTypeDefinitionOf (typeof (IReturn<>));

			IRubyExpressionResult expressionResult;
			if (returnable == null) {
				expressionResult = null;
			} else {
				var resultType = returnable.GetGenericArguments ().FirstOrDefault ();
				expressionResult = (IRubyExpressionResult)Activator.CreateInstance(resultType);
				expressionResult.Parse(e.Data);
			}

			commandsQueue.Remove (commandInQueue);
			commandInQueue.Item3.Invoke (expressionResult);
			activeCommand = null;
		}
	}
}
