﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using EntityState = NetDaemon.Common.EntityState;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Service.App.CodeGeneration
{
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    public partial class NewCodeGenerator : ICodeGenerator
    {
        private string? _nameSpace;

        public string? GenerateCodeRx(string nameSpace, IReadOnlyCollection<EntityState> entities, IReadOnlyCollection<HassServiceDomain> services)
        {
            _nameSpace = nameSpace;
            var orderedEntities = entities.OrderBy(x => x.EntityId);

            var code = CompilationUnit()
                       .AddUsings(UsingDirective(ParseName("System")))
                       .AddUsings(UsingDirective(ParseName("System.Collections.Generic")));

           var namespaceDeclaration = NamespaceDeclaration(ParseName(nameSpace)).NormalizeWhitespace();

           namespaceDeclaration = namespaceDeclaration.AddMembers(GenerateEntityTypes(orderedEntities).ToArray());
           namespaceDeclaration = namespaceDeclaration.AddMembers(GenerateServiceTypes(services.OrderBy(x => x.Domain)).ToArray());
           namespaceDeclaration = namespaceDeclaration.AddMembers(GenerateExtensionMethodClasses(services.OrderBy(x => x.Domain), entities).ToArray());

            code = code.AddMembers(namespaceDeclaration);

            return code.NormalizeWhitespace(Tab.ToString(), eol: "\n").ToFullString();
        }
    }
}