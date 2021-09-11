using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using YamlDotNet.Helpers;
namespace NetDaemon.Service.App.CodeGeneration.Extensions
{
    internal static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> _typeToAliasNames = new()
        {
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(object), "object" },
            { typeof(bool), "bool" },
            { typeof(string), "string" },
            { typeof(int?), "int?" },
            { typeof(uint?), "uint?" },
            { typeof(long?), "long?" },
            { typeof(ulong?), "ulong?" },
            { typeof(float?), "float?" },
            { typeof(double?), "double?" },
            { typeof(bool?), "bool?" },
            { typeof(void), "void" }
        };

        public static string GetCompilableName(this Type type)
        {
            if (_typeToAliasNames.TryGetValue(type, out var friendlyName))
            {
                return friendlyName;
            }

            friendlyName = type.Name;
            if (type.IsGenericType)
            {
                var backtick = friendlyName.IndexOf('`', StringComparison.InvariantCultureIgnoreCase);
                if (backtick > 0)
                {
                    friendlyName = friendlyName.Remove(backtick);
                }
                friendlyName += "<";
                Type[] typeParameters = type.GetGenericArguments();
                for (var i = 0; i < typeParameters.Length; i++)
                {
                    string typeParamName = typeParameters[i].GetCompilableName();
                    friendlyName += (i == 0 ? typeParamName : ", " + typeParamName);
                }
                friendlyName += ">";
            }

            if (type.IsArray)
            {
                return type.GetElementType()?.GetCompilableName() + "[]";
            }

            return friendlyName;
        }


        private static readonly IReadOnlyCollection<(Type Type, string FriendlyName)> _typeToFriendlyNames = new List<(Type, string)>()
        {
            ( typeof(int), "integer" ),
            ( typeof(int?), "integer" ),
            ( typeof(long), "long" ),
            ( typeof(long?), "long" ),
            ( typeof(float), "float" ),
            ( typeof(float?), "float" ),
            ( typeof(double), "double" ),
            ( typeof(double?), "double" ),
            ( typeof(bool), "bool" ),
            ( typeof(bool?), "bool" ),
            ( typeof(string), "string" ),
            ( typeof(DateTime), "date" ),
            ( typeof(DateTime?), "date" ),
            ( typeof(void), "void" ),
            ( typeof(IList), "list" ),
            ( typeof(IDictionary), "dictionary" ),
            ( typeof(object), "object" ),
        };

        public static string GetFriendlyName(this Type type)
        {
            var entry = _typeToFriendlyNames.FirstOrDefault(x => x.Type.IsAssignableFrom(type));

            return entry.FriendlyName;
        }
    }
}