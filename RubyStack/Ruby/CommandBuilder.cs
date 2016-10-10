using System;
using System.Collections.Generic;
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

		public static Dictionary<IReturn, string> BuildCommandsForExpressions (IEnumerable<IReturn> expressions)
		{
			if (expressions == null)
				return null;
			
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

				var matches = Regex.Matches (expressionAttribute.Expression, @"(?<!\{)\{([0-9]+).*?\}(?!})").Cast<Match> ();
				int? placeholdersCount = matches.Count () == 0 ? 0 : matches?.Max (m => int.Parse (m.Groups[1].Value)) + 1;

				if (!placeholdersCount.HasValue || placeholdersCount.Value != args.Count ())
					throw new Exception ($"Number of placeholders in expression \"{expressionAttribute.Expression}\" is less than number of provided pararams - {args.Count ()}");

				expressionsAndCommands.Add (expression, string.Format (expressionAttribute.Expression, args));
			}

			return expressionsAndCommands;
		}
	}
}
