using System;

namespace RubyStack
{
	public static class ReflectionExtensions
	{
		public static Type GetTypeWithGenericTypeDefinitionOf (this Type type, Type genericTypeDefinition)
		{
			var typeInterfaces = type.GetTypeInterfaces ();
			for (int i = 0; i < typeInterfaces.Length; i++) {
				Type type2 = typeInterfaces[i];

				if (type2.IsGenericType && type2.GetGenericTypeDefinition () == genericTypeDefinition)
					return type2;
			}

			var type3 = type.FirstGenericType ();
			if (type3 != null && type3.GetGenericTypeDefinition () == genericTypeDefinition)
				return type3;

			return null;
		}

		public static Type[] GetTypeInterfaces (this Type type)
		{
			return type.GetInterfaces ();
		}

		public static Type FirstGenericType (this Type type)
		{
			while (type != null) {
				if (type.IsGenericType)
					return type;

				type = type.BaseType;
			}

			return null;
		}
	}
}
