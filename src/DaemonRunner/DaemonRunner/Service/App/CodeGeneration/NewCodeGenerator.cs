using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using JoySoftware.HomeAssistant.Model;
using NetDaemon.Common;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using NetDaemon.Service.App.CodeGeneration.Extensions;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Service.App.CodeGeneration
{
    [SuppressMessage("", "CoVariantArrayConversion")]
    public partial class NewCodeGenerator : ICodeGenerator
    {
        private string _nameSpace = null!;

        public string? GenerateCodeRx(string nameSpace, IReadOnlyCollection<EntityState> entities, IReadOnlyCollection<HassServiceDomain> services)
        {
            _nameSpace = nameSpace;

            // entities = entities.Where(x => x.EntityId.Contains("climate")).ToList();
            // services = services.Where(x => x.Domain.Contains("climate")).ToList();

            var orderedEntities = entities.OrderBy(x => x.EntityId);
            var orderedServices = services.OrderBy(x => x.Domain);

            var generatedTypes =
                GenerateEntityTypes(orderedEntities).Concat(
                GenerateServiceTypes(services.OrderBy(x => x.Domain))).Concat(
                GenerateExtensionMethodClasses(orderedServices, entities))
                    .ToArray();

            return CompilationUnit()
                .AddUsings(
                    "System",
                    "System.Collections.Generic",
                    $"{nameof(NetDaemon)}.{nameof(NetDaemon.Common)}")
                .AddNamespace(nameSpace)
                .AddMembers(generatedTypes)
                .ToFullStringNormalized();
        }
    }
}