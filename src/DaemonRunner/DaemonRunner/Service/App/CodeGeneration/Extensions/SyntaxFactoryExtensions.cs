using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.Service.App.CodeGeneration.Extensions
{
    internal static class SyntaxFactoryExtensions
    {
        public static PropertyDeclarationSyntax WithAttribute<T>(this PropertyDeclarationSyntax property, string value) where T: Attribute
        {
            if (property is null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var name = ParseName(typeof(T).FullName!);
            var args = ParseAttributeArgumentList($"(\"{value}\")");
            var attribute = Attribute(name, args);

            return property.WithAttributes(attribute);
        }

        public static CompilationUnitSyntax AddUsings(this CompilationUnitSyntax syntax, params string[]? usings)
        {
            if (usings == null)
                throw new ArgumentNullException(nameof(usings));

            return syntax.AddUsings(usings.Select(u => UsingDirective(ParseName(u))).ToArray());
        }

        public static CompilationUnitSyntax AddNamespace(this CompilationUnitSyntax syntax, string @namespace)
        {
            if (@namespace == null)
                throw new ArgumentNullException(nameof(@namespace));

            return syntax.AddMembers(NamespaceDeclaration(ParseName(@namespace)).NormalizeWhitespace());
        }

        public static string ToFullStringNormalized(this CompilationUnitSyntax syntax)
        {
            return syntax.NormalizeWhitespace(Tab.ToString(), "\n").ToFullString();
        }

        private static PropertyDeclarationSyntax WithAttributes(this PropertyDeclarationSyntax property, params AttributeSyntax[]? attributeSyntaxes)
        {
            var attributes = property.AttributeLists.Add(
                AttributeList(SeparatedList(attributeSyntaxes)).NormalizeWhitespace());

            return property.WithAttributeLists(attributes);
        }
    }
}