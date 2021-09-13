using System;
using System.Text.RegularExpressions;
using NetDaemon.Daemon.Config;
namespace NetDaemon.Service.App.CodeGeneration.Extensions
{
    internal static class StringExtensions
    {
        public static string ToNormalizedPascalCase(this string name, string prefix = "")
        {
            return name.ToPascalCase().ToCompilable(prefix);
        }

        public static string ToNormalizedCamelCase(this string name, string prefix = "")
        {
            return name.ToCamelCase().ToCompilable(prefix);
        }

        public static string ToCompilable(this string name, string prefix = "")
        {
            name = name.Replace(".", "_", StringComparison.InvariantCulture);

            if (!char.IsLetter(name[0]) && name[0] != '_')
            {
                name = prefix + name;
            }

            var stringWithoutSpecialCharacters = Regex.Replace(name, @"[^a-zA-Z0-9_]+", "", RegexOptions.Compiled);

            return stringWithoutSpecialCharacters;
        }
    }
}