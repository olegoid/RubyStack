using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

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

		public static string BuildCommandForExpression (IReturn expression)
		{
			if (expression == null)
				return null;

			var type = expression.GetType ();
			var expressionAttribute = (RubyExpressionAttribute)type.
									GetCustomAttributes (typeof (RubyExpressionAttribute), true).
									FirstOrDefault ();

			if (expressionAttribute == null)
				return null;

			var properties = GetSortedProperties (type);
			var args = properties.Select (p => p.GetValue (expression)).ToArray ();

			var matches = Regex.Matches (expressionAttribute.Expression, @"(?<!\{)\{([0-9]+).*?\}(?!})").Cast<Match> ();
			int? placeholdersCount = matches.Count () == 0 ? 0 : matches?.Max (m => int.Parse (m.Groups[1].Value)) + 1;

			if (!placeholdersCount.HasValue || placeholdersCount.Value != args.Count ())
				throw new Exception ($"Number of placeholders in expression \"{expressionAttribute.Expression}\" is less than number of provided pararams - {args.Count ()}");

			return string.Format (expressionAttribute.Expression, args);
		}
	}
}
