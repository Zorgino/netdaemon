using System;
using System.Collections.Generic;
using System.Linq;
namespace NetDaemon.Service.App.CodeGeneration.Extensions
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> _buildInTypeToAliasNames = new(){
            {typeof(int), "int"},
            {typeof(int?), "int"},
            {typeof(long), "long"},
            {typeof(long?), "long"},
            {typeof(float), "float"},
            {typeof(float?), "float"},
            {typeof(double), "double"},
            {typeof(double?), "double"},
            {typeof(bool), "bool"},
            {typeof(bool?), "bool"},
            {typeof(string), "string"},
            {typeof(DateTime), "DateTime"},
            {typeof(DateTime?), "DateTime"},
            {typeof(void), "void"},
            {typeof(object), "object"}
        };

        public static string GetCompilableName(this Type type)
        {
            if (_buildInTypeToAliasNames.TryGetValue(type, out var friendlyName))
                return friendlyName;

            friendlyName = type.Name;
            if (type.IsGenericType)
            {
                var backtick = friendlyName.IndexOf('`', StringComparison.InvariantCultureIgnoreCase);
                if (backtick > 0)
                    friendlyName = friendlyName.Remove(backtick);
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (var i = 0; i < typeParameters.Length; i++)
                {
                    string typeParamName = typeParameters[i].GetCompilableName();
                    friendlyName += i == 0 ? typeParamName : ", " + typeParamName;
                }
                friendlyName += ">";
            }

            if (type.IsArray)
                return type.GetElementType()?.GetCompilableName() + "[]";

            return friendlyName;
        }

        public static Type GetTypeByValues(this object? obj)
        {
            var defaultType = typeof(string);

            if (obj is null)
                return typeof(object);

            if (TryGetValueType(obj, out var valueType))
                return valueType;

            switch (obj)
            {
                case string:
                    return defaultType;
                case IList<object> list:
                {
                    var listItem = list.FirstOrDefault();

                    Type genericType = listItem is not null
                        ? GetTypeByValues(listItem)
                        : defaultType;

                    return typeof(List<>).MakeGenericType(genericType);
                }
                case Dictionary<string, object> dictionary:
                {
                    var dictionaryValue = dictionary.Values.FirstOrDefault();

                    Type genericType = dictionaryValue is not null
                        ? GetTypeByValues(dictionaryValue)
                        : defaultType;

                    return typeof(Dictionary<,>).MakeGenericType(typeof(string), genericType);
                }
                default: return defaultType;
            }
        }

        public static Type ToTypeCanBeImplicitlyConvertedTo(this Type type)
        {
            if (type == typeof(long))
            {
                return typeof(double);
            }

            if (type == typeof(long?))
            {
                return typeof(double?);
            }

            return type;
        }

        private static bool TryGetValueType(object obj, out Type valueType)
        {
            valueType = null!;

            Type? nullableType = null;

            var objType = obj.GetType();
            if (objType.IsValueType)
                nullableType = objType;
            else if (obj is string str)
                nullableType = str switch{
                    var s when long.TryParse(s, out _) => typeof(long),
                    var s when double.TryParse(s, out _) => typeof(double),
                    var s when bool.TryParse(s, out _) => typeof(bool),
                    var s when DateTime.TryParse(s, out _) => typeof(DateTime),
                    _ => null!
                };

            if (nullableType is not null)
                valueType = nullableType;

            return nullableType is not null;
        }
    }
}