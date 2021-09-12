using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Common.Reactive;
using NetDaemon.Common.Reactive.Services;
using NetDaemon.Daemon.Config;
using NetDaemon.Service.App.CodeGeneration.Extensions;
using NetDaemon.Service.App.CodeGeneration.Helpers;
using static NetDaemon.Service.App.CodeGeneration.Helpers.NamingHelper;
using static NetDaemon.Service.App.CodeGeneration.Helpers.SyntaxFactoryHelper;
using static NetDaemon.Service.App.CodeGeneration.Helpers.TypeHelper;
using OldEntityState = NetDaemon.Common.EntityState;

namespace NetDaemon.Service.App.CodeGeneration
{
    public partial class NewCodeGenerator
    {
        private IEnumerable<TypeDeclarationSyntax> GenerateEntityTypes(IEnumerable<OldEntityState> entities)
        {
            var entityIds = entities.Select(x => x.EntityId).ToList();

            var entityDomains = GetDomainsFromEntities(entityIds).OrderBy(s => s).ToList();

            yield return GenerateRootEntitiesInterface(entityDomains);

            yield return GenerateRootEntitiesClass(entityDomains);

            foreach (var entityClass in entityDomains.Select(entityDomain => GenerateEntityDomainType(entityDomain, entityIds)))
            {
                yield return entityClass;
            }

            foreach (var entityDomain in entityDomains)
            {
                yield return GenerateEntityType(entityDomain);
            }

            foreach (var attributeRecord in GenerateEntityAttributeRecords(entities))
            {
                yield return attributeRecord;
            }
        }
        private static TypeDeclarationSyntax GenerateRootEntitiesInterface(IEnumerable<string> domains)
        {
            var autoProperties = domains.Select(domain =>
            {
                var typeName = GetEntitiesTypeName(domain);
                var propertyName = domain.ToPascalCase();

                return Property(typeName, propertyName, set: false);
            }).ToArray();

            return Interface("IEntities").AddMembers(autoProperties).ToPublic();
        }

        private static TypeDeclarationSyntax GenerateRootEntitiesClass(IEnumerable<string> domains)
        {
            var haContextNames = GetNames<INetDaemonRxApp>();

            var properties = domains.Select(domain =>
            {
                var entitiesTypeName = GetEntitiesTypeName(domain);
                var entitiesPropertyName = domain.ToPascalCase();

                return ParseProperty($"{entitiesTypeName} {entitiesPropertyName} => new(_{haContextNames.VariableName});").ToPublic();
            }).ToArray();

            return ClassWithInjected<INetDaemonRxApp>("Entities").WithBase((string)"IEntities").AddMembers(properties).ToPublic();
        }

        private IEnumerable<TypeDeclarationSyntax> GenerateEntityAttributeRecords(IEnumerable<OldEntityState> entities)
        {
            foreach (var entityDomainGroups in entities.GroupBy(x => EntityIdHelper.GetDomain(x.EntityId)))
            {
                var domain = entityDomainGroups.Key;
                var attributesTypeName = GetAttributesTypeName(domain);

                var attrs = new List<(string HaName, Type Type)>();

                foreach (var entity in entityDomainGroups)
                {
                    var domainEntities = new Dictionary<string, object>(entity.Attribute);

                    foreach (var (attrName, attrObject) in domainEntities)
                    {
                        var attrType = TypeHelper.GetType(attrObject);
                        if (attrs.Any(attr => attr.HaName == attrName && attr.Type == attrType))
                        {
                            continue;
                        }

                        attrs.Add((attrName, attrType));
                    }
                }

                // var conflicts = attrs.Duplicates(x => new { x.HaName, x.Type });
                var sameNameButDifferentTypesConflicts = attrs
                    .Duplicates(x => x.HaName.ToNormalizedPascalCase())
                    .Where(nameDuplicates => nameDuplicates.GroupBy(x => x.Type).Count() > 1);

                if (sameNameButDifferentTypesConflicts.Any())
                {
                    Console.WriteLine($@"""{domain}"" domain has attributes with same name but different types. To handle those, create:");

                    Console.WriteLine($"using {_nameSpace} \n {{");
                    Console.WriteLine($"public partial record {attributesTypeName} \n {{");

                    foreach (var conflict in sameNameButDifferentTypesConflicts)
                    {
                        Console.WriteLine($"public [TYPE] {conflict.Key.ToNormalizedPascalCase()} {{get; set;}}");

                    }

                    attrs.RemoveAll(x => sameNameButDifferentTypesConflicts.Any(conflict => conflict.Key == x.HaName));
                }

                foreach (var conflict in sameNameButDifferentTypesConflicts)
                {

                }

                IEnumerable<(string Name, string TypeName, string SerializationName)> autoPropertiesParams = attrs
                    .Select(a => (a.HaName.ToNormalizedPascalCase(), a.Type.GetCompilableName() + "?", a.HaName));

                var autoProperties = autoPropertiesParams.Select(a =>
                    Property(a.TypeName, a.Name).ToPublic().WithAttribute<JsonPropertyNameAttribute>(a.SerializationName))
                    .ToArray();

                yield return Record(attributesTypeName, autoProperties).ToPublic();
            }
        }

        private static TypeDeclarationSyntax GenerateEntityDomainType(string domain, IEnumerable<string> entities)
        {
            var entityClass = ClassWithInjected<INetDaemonRxApp>(GetEntitiesTypeName(domain)).ToPublic();

            var domainEntities = entities.Where(EntityIsOfDomain(domain)).ToList();

            var entityProperty = domainEntities.Select(entityId => GenerateEntityProperty(entityId, domain)).ToArray();

            return entityClass.AddMembers(entityProperty);
        }

        private static Func<string, bool> EntityIsOfDomain(string domain)
        {
            return n => n.StartsWith(domain + ".", StringComparison.InvariantCultureIgnoreCase);
        }

        private static PropertyDeclarationSyntax GenerateEntityProperty(string entityId, string domain)
        {
            var entityName = EntityIdHelper.GetEntity(entityId);

            var propertyCode = $@"{GetDomainEntityTypeName(domain)} {entityName.ToNormalizedPascalCase((string)"E_")} => new(_{GetNames<INetDaemonRxApp>().VariableName}, ""{entityId}"");";

            return ParseProperty(propertyCode).ToPublic();
        }

        private static TypeDeclarationSyntax GenerateEntityType(string domain)
        {
            string attributesGeneric = GetAttributesTypeName(domain);

            var entityClass = $"{GetDomainEntityTypeName(domain)}";

            var baseClass = $"{typeof(RxEntityBase).FullName}<{entityClass}, {typeof(OldEntityState).FullName}<string, {attributesGeneric}>, string, {attributesGeneric}>";

            var (className, variableName) = GetNames<INetDaemonRxApp>();
            var classDeclaration = $@"class {entityClass} : {baseClass}
                                    {{
                                            public {domain.ToPascalCase()}Entity({className} {variableName}, params string[] entityIds) : base({variableName}, entityIds)
                                            {{
                                            }}
                                    }}";

            return ParseClass(classDeclaration).ToPublic();
        }
        /// <summary>
        ///     Returns a list of domains from all entities
        /// </summary>
        /// <param name="entities">A list of entities</param>
        internal static IEnumerable<string> GetDomainsFromEntities(IEnumerable<string> entities) => entities.Select(EntityIdHelper.GetDomain).Distinct();
    }
}