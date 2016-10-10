using System;

using RubyStack;
using NUnit.Framework;

namespace Test
{
	[RubyExpression ("puts \"Hello World!\"")]
	class ExpressionWithoutParameters : IReturnVoid
	{
	}

	class NotAttributedExpression : IReturnVoid
	{
	}

	[RubyExpression ("puts \"{1} {0}\"")]
	class ReverseParametersExpression : IReturnVoid
	{
		[RubyStack.Order (0)]
		public string FirstParam { get; set; }

		[RubyStack.Order (1)]
		public string SecondParam { get; set; }
	}

	[RubyExpression ("puts \"{0} {3}\"")]
	class NotFarfetchedExpression : IReturnVoid
	{
		[RubyStack.Order (0)]
		public string FirstParam { get; set; }

		[RubyStack.Order (1)]
		public string SecondParam { get; set; }

		[RubyStack.Order (2)]
		public string ThirdParam { get; set; }

		[RubyStack.Order (3)]
		public string FourthParam { get; set; }
	}

	[RubyExpression ("puts \"{0} {1}\"")]
	class CorrectOrderExpression : IReturnVoid
	{
		[RubyStack.Order (0)]
		public string FirstParam { get; set; }

		[RubyStack.Order (1)]
		public string SecondParam { get; set; }
	}

	[TestFixture]
	public class CommandBuilderTests
	{
		[Test]
		public void BuildCommands_NullParam ()
		{
			Assert.IsNull (CommandBuilder.BuildCommandsForExpressions (null),
					"BuildCommandsForExpressions should return null");
		}

		[Test]
		public void BuildCommands_EmptyParam ()
		{
			Assert.IsTrue (CommandBuilder.BuildCommandsForExpressions (new IReturn[] { }).Count == 0,
					"BuildCommandsForExpressions should return empty dictionary");
		}

		[Test]
		public void BuildCommands_ExpressionWithoutParameters ()
		{
			string expectedCommand = "puts \"Hello World!\"";
			var expression = new ExpressionWithoutParameters ();

			var commands = CommandBuilder.BuildCommandsForExpressions (new IReturn[] { expression });
			Assert.IsTrue (commands.Count == 1 && commands[expression] == expectedCommand,
					"BuildCommandsForExpressions failed to build command for expression without params");
		}

		[Test]
		public void BuildCommands_ReverseParametersExpression ()
		{
			string expectedCommand = "puts \"Hello World!\"";
			var expression = new ReverseParametersExpression {
				FirstParam = "World!",
				SecondParam = "Hello"
			};

			var commands = CommandBuilder.BuildCommandsForExpressions (new IReturn[] { expression });
			Assert.IsTrue (commands.Count == 1 && commands[expression] == expectedCommand,
					"BuildCommandsForExpressions failed to build command for expression with reversed params");
		}

		[Test]
		public void BuildCommands_NotAttributedExpression ()
		{
			var expression = new NotAttributedExpression ();
			var commands = CommandBuilder.BuildCommandsForExpressions (new IReturn[] { expression });
			Assert.IsTrue (commands.Count == 0,
					"BuildCommandsForExpressions failed to build command for not attributed expression");
		}

		[Test]
		public void BuildCommands_NotFarfetchedExpression ()
		{
			string expectedCommand = "puts \"Hello World!\"";
			var expression = new NotFarfetchedExpression {
				FirstParam = "Hello",
				FourthParam = "World!"
			};

			var commands = CommandBuilder.BuildCommandsForExpressions (new IReturn[] { expression });
			Assert.IsTrue (commands.Count == 1 && commands[expression] == expectedCommand,
					"BuildCommandsForExpressions failed to build command for not farfetched expression");
		}

		[Test]
		public void BuildCommands_CorrectOrderExpression ()
		{
			string expectedCommand = "puts \"Hello World!\"";
			var expression = new CorrectOrderExpression {
				FirstParam = "Hello",
				SecondParam = "World!"
			};

			var commands = CommandBuilder.BuildCommandsForExpressions (new IReturn[] { expression });
			Assert.IsTrue (commands.Count == 1 && commands[expression] == expectedCommand,
					"BuildCommandsForExpressions failed to build command for expression with correct order of params");
		}
	}
}
