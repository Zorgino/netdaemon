﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

                var attrs = new List<HaAttributeProperty>();

                foreach (var entity in entityDomainGroups)
                {
                    var domainEntities = new Dictionary<string, object>(entity.Attribute);

                    foreach (var (attrName, attrObject) in domainEntities)
                    {
                        var attrType = TypeHelper.GetType(attrObject);

                        attrs.Add(new HaAttributeProperty(attrName, attrType));
                    }
                }

                attrs = attrs.Distinct().ToList();

                var conflictingHaNames = attrs
                    .Duplicates(x => x.HaName)
                    .Where(nameDuplicates => nameDuplicates.GroupBy(x => x.TypeName).Count() > 1)
                    .Select(x => x.Key)
                    .ToList();

                var autoProperties = new List<PropertyDeclarationSyntax>();

                if (conflictingHaNames.Any())
                {
                    Console.WriteLine($@"""{domain}"" domain has attributes with same name but different types. To handle those, create:");

                    Console.WriteLine($"public partial record {attributesTypeName} \n {{");

                    foreach (var conflictHaName in conflictingHaNames)
                    {
                        Console.WriteLine($"public TYPE {conflictHaName.ToNormalizedPascalCase()} => (TYPE){"@_" + conflictHaName.ToCompilable()}");
                    }

                    attrs.RemoveAll(x => conflictingHaNames.Any(conflict => conflict == x.HaName));
                }

                //TODO: fix this trash
                attrs.RemoveAll(x => attrs.Duplicates(y => y.HaName.ToNormalizedPascalCase()).ToList().Exists(z => z.Key == x.HaName.ToNormalizedPascalCase()));

                attrs = attrs.OrderBy(a => a.HaName).ToList();

                autoProperties.AddRange(
                    attrs.OrderBy(a => a.PropName)
                    .Select(a => PropertyComputed(a.TypeName, a.PropName, $"{a.BackingPropName}.{nameof(Common.NetDaemonExtensions.ToObject)}<{a.TypeName}>()")
                    .ToPublic()));

                autoProperties.AddRange(
                    attrs
                    .Select(a => Property(typeof(JsonElement).FullName!, a.BackingPropName)
                    .ToPublic()
                    .WithAttribute<JsonPropertyNameAttribute>(a.HaName)));

                yield return Record(attributesTypeName, autoProperties).ToPublic();
            }
        }

        record HaAttributeProperty
        {
            public HaAttributeProperty()
            {
            }
            public HaAttributeProperty(string haName, Type type)
            {
                HaName = haName;
                Type = type;
            }

            public string HaName { get; set; }

            public Type Type { get; set; }

            public string TypeName => Type.GetCompilableName() + "?";

            public string PropName => HaName.ToNormalizedPascalCase();

            public string BackingPropName => "_" + HaName.ToCompilable();
        }

        // private class EntitiesAttributes
        // {
        //     private readonly IEnumerable<EntityAttributes> _entityAttributes;
        //     public EntitiesAttributes(IEnumerable<OldEntityState> entities)
        //     {
        //     }
        // }
        //
        // private class EntityAttributes
        // {
        //     private readonly Dictionary<string, object> _attributes;
        //     public EntityAttributes(Dictionary<string, object> attributes)
        //     {
        //         _attributes = attributes;
        //     }
        // }

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

            var propertyCode = $@"{GetDomainEntityTypeName(domain)} {entityName.ToNormalizedPascalCase("E_")} => new(_{GetNames<INetDaemonRxApp>().VariableName}, ""{entityId}"");";

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