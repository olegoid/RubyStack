using System;
using System.Collections.Generic;
using System.Linq;

namespace RubyStack
{
	public class RubyEngine
	{
		readonly ProcessRunner processRunner;

		public RubyEngine ()
		{
			processRunner = new ProcessRunner ();
		}

		public IRubyExpressionResult Run (IReturn expression)
		{
			return Run (new [] { expression }).FirstOrDefault ().Value;
		}

		public Dictionary<IReturn, IRubyExpressionResult> Run (IEnumerable<IReturn> expressions)
		{
			var expressionsAndCommands = CommandBuilder.BuildCommandsForExpressions (expressions);
			var commands = expressionsAndCommands.Values.ToList ();
			commands.Add ("exit");

			ProcessResult processResult = processRunner.Run ("irb", stdinCommands: commands.ToArray ());

			var results = new Dictionary<IReturn, IRubyExpressionResult> ();

			for (int i = 0; i < processResult.Output.Length; i++) {
				var exprAndCmd = expressionsAndCommands.FirstOrDefault (c => c.Value == processResult.Output[i].Data);
				var expr = exprAndCmd.Key;

				if (expr == null)
					continue;

				var returnable = expr.GetType ().GetTypeWithGenericTypeDefinitionOf (typeof (IReturn<>));

				// In this case we probably dealing with IReturn or IReturnVoid
				if (returnable == null)
					results.Add (expr, null);

				var resultType = returnable.GetGenericArguments ().FirstOrDefault ();
				var expressionResult = (IRubyExpressionResult)Activator.CreateInstance (resultType);

				var output = processResult.Output [i + 1];
				expressionResult.Parse (output.Data);

				results.Add (expr, expressionResult);
			}

			return results;
		}
	}
}
