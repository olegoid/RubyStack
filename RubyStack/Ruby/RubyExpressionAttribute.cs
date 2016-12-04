using System;

namespace RubyStack
{
	/// <summary>
	/// Ruby expression attribute. Usually contains ruby statement.
	/// </summary>
	[AttributeUsage (AttributeTargets.Class)]
	public class RubyExpressionAttribute : Attribute
	{
		public string Expression { get; private set; }

		public RubyExpressionAttribute (string expression)
		{
			if (string.IsNullOrEmpty (expression))
				throw new ArgumentNullException (nameof (expression));

			Expression = expression;
		}
	}
}
