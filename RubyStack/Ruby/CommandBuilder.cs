using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RubyStack
{
	public static class CommandBuilder
	{
		static IOrderedEnumerable<PropertyInfo> GetSortedProperties (Type type)
		{
			return type.GetProperties ().
						OrderBy (p => ((OrderAttribute)p.GetCustomAttributes (typeof (OrderAttribute), false).FirstOrDefault ()).
						Order);
		}

		public static Dictionary<IReturn, string> BuildCommandsForExpressions (IEnumerable<IReturn> expressions)
		{
			var expressionsAndCommands = new Dictionary<IReturn, string> ();
			foreach (var expression in expressions) {
				var type = expression.GetType ();
				var expressionAttribute = (RubyExpressionAttribute)type.
										GetCustomAttributes (typeof (RubyExpressionAttribute), true).
										FirstOrDefault ();

				if (expressionAttribute == null)
					continue;

				var properties = GetSortedProperties (type);
				var args = properties.Select (p => p.GetValue (expression)).ToArray ();
				expressionsAndCommands.Add (expression, string.Format (expressionAttribute.Expression, args));
			}

			return expressionsAndCommands;
		}
	}
}
