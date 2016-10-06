using System;

namespace RubyStack
{
	[AttributeUsage (AttributeTargets.Class)]
	public class RubyExpressionAttribute : Attribute
	{
		public string Expression { get; private set; }

		public RubyExpressionAttribute (string expression)
		{
			Expression = expression;
		}
	}
}
