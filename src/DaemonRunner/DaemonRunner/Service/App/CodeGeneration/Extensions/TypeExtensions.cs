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
        private static readonly Dictionary<Type, string> _buildInTypeToAliasNames = new()
        {
            { typeof(int), "int"  },
            { typeof(int?), "int" },
            { typeof(long), "long"  },
            { typeof(long?), "long" },
            { typeof(float), "float"  },
            { typeof(float?), "float" },
            { typeof(double), "double"  },
            { typeof(double?), "double" },
            { typeof(bool), "bool"  },
            { typeof(bool?), "bool" },
            { typeof(string), "string"  },
            { typeof(DateTime), "DateTime"  },
            { typeof(DateTime?), "DateTime" },
            { typeof(void), "void"  },
            { typeof(object), "object"  },
        };

        public static string GetCompilableName(this Type type)
        {
            if (_buildInTypeToAliasNames.TryGetValue(type, out var friendlyName))
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

        private static bool TryGetBuildInTypeFriendlyName(Type type, out string? name)
        {
            name = null;

            if (type == typeof(int) && type == typeof(int?))
            {
                name = "integer";
            }
            else if (type == typeof(DateTime) && type == typeof(DateTime?))
            {
                name = "date";
            }
            else if (_buildInTypeToAliasNames.TryGetValue(type, out var typeName))
            {
                name = typeName;
            }
            else if (type.Namespace is "System")
            {
                name = typeName;
            }

            return name is not null;
        }

        public static string GetFriendlyName(this Type type)
        {
            if (TryGetBuildInTypeFriendlyName(type, out var buildInTypeName))
            {
                return buildInTypeName!;
            }

            if (TryGetCollectionFriendlyName(type, out var collectionTypeName))
            {
                return collectionTypeName!;
            }

            throw new ArgumentException("Unknown type", nameof(type));
        }

        private static bool TryGetCollectionFriendlyName(Type collectionType, out string? name)
        {
            name = null;
            if (!collectionType.IsAssignableTo(typeof(IEnumerable)) && !collectionType.IsAssignableTo(typeof(IDictionary)))
            {
                return false;
            }

            string colllectionName;
            string collectionGenericTypeName;

            if (collectionType.IsAssignableTo(typeof(IDictionary)))
            {
                colllectionName = "Dictionary";
                collectionGenericTypeName = $"{collectionType.GetGenericArguments()[0].GetFriendlyName()!.ToNormalizedPascalCase()}" +
                                            $"{collectionType.GetGenericArguments()[1].GetFriendlyName()!.ToNormalizedPascalCase()}";
            }
            else
            {
                colllectionName = "List";
                collectionGenericTypeName = $"{collectionType.GetGenericArguments()[0].GetFriendlyName()!.ToNormalizedPascalCase()}";
            }

            name = $"{colllectionName}Of{collectionGenericTypeName}";

            return true;
        }
    }
}