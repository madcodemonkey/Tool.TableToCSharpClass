using System;

namespace AdvExample1
{
    /// <summary>Reflect create extensions.</summary>
    public static class ReflectionCreateExtensions
    {
        /// <summary>Creates a class (someType) and cast it to an interface (TInterface).</summary>
        public static TInterface HelpCreateAndCastToInterface<TInterface>(this Type someType, string optionalMessage = "")
        {
            if (someType == null)
                throw new ArgumentNullException("Please specify a type!  {optionalMessage}");
            if (someType.IsClass == false)
                throw new ArgumentException($"The {someType.Name} type is not a class!  {optionalMessage}");
            if (typeof(TInterface).IsAssignableFrom(someType) == false)
                throw new ArgumentException($"The {someType.Name} class does not implement the {typeof(TInterface).Name} interface!  {optionalMessage}");
            return (TInterface)Activator.CreateInstance(someType);
        }

        /// <summary>Checks to see if the type is a nullable (e.g., int?)</summary>
        /// <param name="someType">Type to check</param>
        public static bool HelpIsNullable(this Type someType)
        {
            if (someType.IsGenericType == false)
                return false;
            return someType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>Converts a type in a friendly string.  Normally, you can use just the name property on 
        /// the Type class, but with nullables you have to look at the underlying type as well.</summary>
        /// <param name="someType">Type to show as a string</param>
        public static string HelpTypeToString(this Type someType)
        {
            bool helpIsNullable = someType.HelpIsNullable();
            string result = helpIsNullable ? Nullable.GetUnderlyingType(someType).Name : someType.Name;

            result = ConvertToCommonType(result);


            return helpIsNullable ? $"{result}?" : result;
        }

        /// <summary>Converts the formal type names to informal/common type names.</summary>
        /// <param name="someType"></param>
        /// <returns></returns>
        private static string ConvertToCommonType(string someType)
        {
            switch (someType)
            {
                case "Boolean":
                    return "bool";
                case "Byte":
                    return "byte";
                case "Int16":
                    return "short";
                case "Int32":
                    return "int";
                case "Int64":
                    return "long";
                case "String":
                    return "string";
                default:
                    return someType;
            }
        }
    }
}
