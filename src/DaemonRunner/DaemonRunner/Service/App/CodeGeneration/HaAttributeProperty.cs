using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Service.App.CodeGeneration.Extensions;
using NetDaemon.Service.App.CodeGeneration.Helpers;
namespace NetDaemon.Service.App.CodeGeneration
{
    internal record HaAttributeProperty
    {
        public HaAttributeProperty(string haName, Type type)
        {
            HaName = haName;
            Type = type;
        }

        public string HaName { get; set; }

        public Type Type { get; set; }

        public string TypeName => Type.GetCompilableName() + "?";

        public string PropName => GetPropName(HaName);

        public string BackingPropName => "_" + HaName.ToCompilable();

        public  PropertyDeclarationSyntax GetAutoProperty() => SyntaxFactoryHelper.Property(TypeName, PropName)
            .WithAttribute<JsonPropertyNameAttribute>(HaName);

        public  PropertyDeclarationSyntax GetComputedProperty() => SyntaxFactoryHelper.PropertyComputed(TypeName,
            PropName,
            $"{BackingPropName}.{nameof(Common.NetDaemonExtensions.ToObject)}<{TypeName}>()");

        public PropertyDeclarationSyntax GetJsonElementProperty() => SyntaxFactoryHelper.Property(typeof(JsonElement).FullName!,
            BackingPropName).WithAttribute<JsonPropertyNameAttribute>(HaName);

        public static string GetPropName(string haName)
        {
            return haName.ToNormalizedPascalCase();
        }
    }
}