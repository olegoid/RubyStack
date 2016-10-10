using System;

using RubyStack;
using NUnit.Framework;

namespace Test
{
	[TestFixture]
	public class RubyExpressionAttributeTests
	{
		[Test]
		public void Ctor_NullArg_ShouldThrow ()
		{
			Assert.Throws<ArgumentNullException> (() => {
				new RubyExpressionAttribute (null);
			}, "RubyExpression .ctor should throw exceptions for null arguments");
		}

		[Test]
		public void Ctor_EmptyStringArg_ShouldThrow ()
		{
			Assert.Throws<ArgumentNullException> (() => {
				new RubyExpressionAttribute (string.Empty);
			}, "RubyExpression .ctor should throw exceptions when argument is an empty string");
		}
	}
}
