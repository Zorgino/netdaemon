using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.Service.App.CodeGeneration.Helpers
{
    internal static class SyntaxFactoryHelper
    {
        public static GlobalStatementSyntax ParseMethod(string code)
        {
            return Parse<GlobalStatementSyntax>(code);
        }

        public static PropertyDeclarationSyntax ParseProperty(string code)
        {
            return Parse<PropertyDeclarationSyntax>(code);
        }

        public static RecordDeclarationSyntax ParseRecord(string code)
        {
            return Parse<RecordDeclarationSyntax>(code);
        }

        public static PropertyDeclarationSyntax Property(string typeName, string propertyName, bool set = true)
        {
            return ParseProperty($"{typeName} {propertyName} {{ get; {(set ? "set; " : string.Empty)}}}");
        }

        public static PropertyDeclarationSyntax PropertyComputed(string typeName, string propertyName, string computedString)
        {
            return ParseProperty($"{typeName} {propertyName} => {computedString};");
        }

        public static SyntaxTriviaList ParseComments(params string[] text)
        {
            return TriviaList(text.Select(Comment));
        }

        public static ClassDeclarationSyntax ClassWithInjected<TInjected>(string className, bool @base = false)
        {
            var (typeName, variableName) = NamingHelper.GetNames<TInjected>();

            var classCode = $@"class {className}
                          {{
                              private readonly {typeName} _{variableName};

                              public {className}( {typeName} {variableName}) {(@base ? $": base({variableName})" : null)}
                              {{
                                  _{variableName} = {variableName};
                              }}
                          }}";

            return ParseClass(classCode);
        }

        public static ClassDeclarationSyntax Class(string name)
        {
            return ClassDeclaration(name);
        }

        public static TypeDeclarationSyntax Interface(string name)
        {
            return InterfaceDeclaration(name);
        }

        public static RecordDeclarationSyntax Record(string name, IEnumerable<MemberDeclarationSyntax> properties)
        {
            return RecordDeclaration(Token(SyntaxKind.RecordKeyword), name)
                .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                .AddMembers(properties.ToArray())
                .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
        }

        public static RecordDeclarationSyntax RecordCommented(string name, params string[] comments)
        {
            return RecordDeclaration(Token(SyntaxKind.RecordKeyword), name)
                .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
                .WithCloseBraceToken(Token(ParseComments(comments), SyntaxKind.CloseBraceToken, TriviaList()));
        }

        public static T ToPublic<T>(this T member)
            where T: MemberDeclarationSyntax
        {
            return (T)member.AddModifiers(Token(SyntaxKind.PublicKeyword));
        }

        public static T ToPrivate<T>(this T member)
            where T: MemberDeclarationSyntax
        {
            return (T)member.AddModifiers(Token(SyntaxKind.PrivateKeyword));
        }

        public static T ToPartial<T>(this T member)
            where T: MemberDeclarationSyntax
        {
            return (T)member.AddModifiers(Token(SyntaxKind.PartialKeyword));
        }

        public static T ToStatic<T>(this T member)
            where T: MemberDeclarationSyntax
        {
            return (T)member.AddModifiers(Token(SyntaxKind.StaticKeyword));
        }

        public static T WithBase<T>(this T member, string baseTypeName)
            where T: TypeDeclarationSyntax
        {
            return (T)member.WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName(baseTypeName)))));
        }

        private static T Parse<T>(string text)
        {
            var node = CSharpSyntaxTree.ParseText(text).GetRoot().ChildNodes().OfType<T>().FirstOrDefault();

            if (node is null)
                throw new ArgumentException($@"Text ""{text}"" contains invalid code", nameof(text));

            return node;
        }

        public static ClassDeclarationSyntax ParseClass(string code)
        {
            return Parse<ClassDeclarationSyntax>(code);
        }
    }
}