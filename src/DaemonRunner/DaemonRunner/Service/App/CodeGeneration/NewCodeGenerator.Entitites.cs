using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;
using NetDaemon.Common.Reactive.Services;
using NetDaemon.Common.Reactive.States;
using NetDaemon.Daemon.Config;
using NetDaemon.Service.App.CodeGeneration.Extensions;
using NetDaemon.Service.App.CodeGeneration.Helpers;
using static NetDaemon.Service.App.CodeGeneration.Helpers.NamingHelper;
using static NetDaemon.Service.App.CodeGeneration.Helpers.SyntaxFactoryHelper;
using OldEntityState = NetDaemon.Common.EntityState;

namespace NetDaemon.Service.App.CodeGeneration
{
    public partial class NewCodeGenerator
    {
        protected virtual Type? GetDomainEntityStateType(string domain)
        {
            return null;
        }

        protected virtual Type? GetEntityStateType(string domain)
        {
            return null;
        }

        private IEnumerable<TypeDeclarationSyntax> GenerateEntityTypes(IEnumerable<IEntityProperties> entities)
        {
            var entityIds = entities.Select(x => x.EntityId).ToList();
            var entityDomains = GetDomainsFromEntities(entityIds).OrderBy(s => s).ToList();

            return new[]
            {
                new []
                {
                    GenerateRootEntitiesInterface(entityDomains),
                    GenerateRootEntitiesClass(entityDomains)
                },

                GenerateEntitiesTypes(entityIds),
                GenerateEntityDomainTypes(entityDomains),
                GenerateEntityDomainAttributeRecords(entities),
                GenerateEntityAttributeRecords(entities)
            }.SelectMany(x => x);
        }

        private IEnumerable<TypeDeclarationSyntax> GenerateEntityDomainTypes(IEnumerable<string> domains)
        {
            return domains.SelectMany(GenerateEntityDomainType);
        }

        private IEnumerable<TypeDeclarationSyntax> GenerateEntityDomainType(string domain)
        {
            return new[]{
                GenerateEntityDomainCommonType(domain),
                GenerateEntityDomainGenericAttributesType(domain),
                GenerateEntityDomainGenericStateAndAttributesType(domain)
            }.Select(x => x.ToPublic().ToPartial());
        }

        private static TypeDeclarationSyntax GenerateEntityDomainCommonType(string domain)
        {
            var entityClass = GetDomainEntityTypeName(domain);
            var (className, variableName) = GetNames<INetDaemonRxApp>();

            var baseClass = $"{GetDomainEntityTypeName(domain)}<{GetAttributesTypeName(domain)}>";
            var classDeclaration = $@"class {entityClass} : {baseClass}
                                    {{
                                            public {entityClass}({className} {variableName}, params string[] entityIds) : base({variableName}, entityIds)
                                            {{
                                            }}
                                    }}";

            return ParseClass(classDeclaration);
        }

        private TypeDeclarationSyntax GenerateEntityDomainGenericAttributesType(string domain)
        {
            var entityClass = GetDomainEntityTypeName(domain);

            var attributesGeneric = "TAttributes";
            var entityGenericClass = $"{entityClass}<{attributesGeneric}>";
            var (className, variableName) = GetNames<INetDaemonRxApp>();

            var stateType = GetDomainEntityStateType(domain) ?? typeof(StringState);

            var baseClass = $"{GetDomainEntityTypeName(domain)}<{stateType},{attributesGeneric}>";
            var classDeclaration = $@"class {entityGenericClass} : {baseClass}
                                            {GetReferenceTypeConstraint(attributesGeneric)}
                                    {{
                                            public {entityClass}({className} {variableName}, params string[] entityIds) : base({variableName}, entityIds)
                                            {{
                                            }}
                                    }}";

            return ParseClass(classDeclaration);
        }

        private static TypeDeclarationSyntax GenerateEntityDomainGenericStateAndAttributesType(string domain)
        {
            var entityClass = GetDomainEntityTypeName(domain);
            var (className, variableName) = GetNames<INetDaemonRxApp>();
            var statesGeneric = "TState";
            var attributesGeneric = "TAttributes";
            var entityGenericClass = $"{entityClass}<{statesGeneric}, {attributesGeneric}>";

            string baseClass = $"{typeof(RxEntityBase).FullName}<{entityGenericClass}, {typeof(EntityState).FullName}<{statesGeneric}, {attributesGeneric}>, {statesGeneric}, {attributesGeneric}>";
            string classDeclaration = $@"class {entityGenericClass} : {baseClass}
                                            {GetReferenceTypeConstraint(attributesGeneric)}
                                            {GetReferenceTypeConstraint(statesGeneric)}
                                    {{
                                            public {entityClass}({className} {variableName}, params string[] entityIds) : base({variableName}, entityIds)
                                            {{
                                            }}
                                    }}";

            return ParseClass(classDeclaration);
        }



        private static TypeDeclarationSyntax GenerateRootEntitiesInterface(IEnumerable<string> domains)
        {
            var autoProperties = domains.Select(domain =>
            {
                var typeName = GetEntitiesTypeName(domain);
                var propertyName = domain.ToPascalCase();

                return Property(typeName, propertyName, set: false);
            }).ToArray();

            return Interface("IEntities")
                .AddMembers(autoProperties)
                .ToPublic()
                .ToPartial();
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

            return ClassWithInjected<INetDaemonRxApp>("Entities").WithBase((string)"IEntities")
                .AddMembers(properties)
                .ToPublic()
                .ToPartial();
        }

        private IEnumerable<TypeDeclarationSyntax> GenerateEntityAttributeRecords(IEnumerable<IEntityProperties> entities)
        {
            return entities.Select(x => GenerateEntityAttributeRecord(x, GetAttributesTypeName(x.EntityId)));
        }

        private TypeDeclarationSyntax GenerateEntityAttributeRecord(IEntityProperties entity, string attributesTypeName)
        {
            var autoProperties = new Dictionary<string, object>(entity.Attribute)
                .Select(x => new HaAttributeProperty(x.Key, x.Value.GetTypeByValues()))
                .OrderBy(a => a.HaName)
                .Distinct()
                .Select(a => a.GetAutoProperty().ToPublic());

            return Record(attributesTypeName, autoProperties).ToPublic().ToPartial();
        }

        private IEnumerable<TypeDeclarationSyntax> GenerateEntityDomainAttributeRecords(IEnumerable<IEntityProperties> entities)
        {
            var mainRecords = new List<TypeDeclarationSyntax>();
            var partialCommentedRecords = new List<TypeDeclarationSyntax>();

            foreach (var entityDomainGroups in entities.GroupBy(x => EntityIdHelper.GetDomain(x.EntityId)))
            {
                var attributesTypeName = GetAttributesTypeName(entityDomainGroups.Key);

                var attrs = entityDomainGroups
                    .Select(es => new Dictionary<string, object>(es.Attribute))
                    .SelectMany(x => x)
                    .Select(x => new HaAttributeProperty(x.Key, x.Value.GetTypeByValues().ToTypeCanBeImplicitlyConvertedTo()))
                    .OrderBy(a => a.HaName)
                    .Distinct()
                    .ToList();

                var conflictingHaAttributes = GetConflicts(attrs).ToList();

                var autoProperties = new List<PropertyDeclarationSyntax>();

                autoProperties.AddRange(
                    attrs
                    .Except(conflictingHaAttributes)
                    .Select(a => a.GetComputedProperty().ToPublic())
                );

                autoProperties.AddRange(
                    attrs
                    .DistinctBy(x => x.HaName)
                    .Select(x => x.GetJsonElementProperty().ToPublic())
                );

                mainRecords.Add(Record(attributesTypeName, autoProperties));

                if (conflictingHaAttributes.Count == 0)
                {
                    continue;
                }

                var commentedProperties = conflictingHaAttributes.Select(x => "// public " + x.GetComputedProperty().ToFullString()).ToArray();

                partialCommentedRecords.Add(RecordCommented(attributesTypeName, commentedProperties));
            }

            return mainRecords.Concat(partialCommentedRecords).Select(t => t.ToPublic().ToPartial());
        }

        private static IEnumerable<HaAttributeProperty> GetConflicts(IEnumerable<HaAttributeProperty> attrs)
        {
            var result = new List<HaAttributeProperty>();

            result.AddRange(
            attrs
                .Duplicates(x => x.HaName)
                .Where(nameDuplicates => nameDuplicates.GroupBy(x => x.TypeName).Count() > 1)
                .SelectMany(x => x)
            );

            result.AddRange(
                attrs
                    .Duplicates(x => x.PropName)
                    .SelectMany(x => x)
                );

            return result.Distinct();
        }

        private IEnumerable<TypeDeclarationSyntax> GenerateEntitiesTypes(IEnumerable<string> entityIds)
        {
            return entityIds.GroupBy(EntityIdHelper.GetDomain, GenerateEntitiesType);
        }

        private TypeDeclarationSyntax GenerateEntitiesType(string domain, IEnumerable<string> entityIds)
        {
            var baseClass = $"{GetDomainEntityTypeName(domain)}<{GetAttributesTypeName(domain)}>";
            var entityClass = ClassWithInjected<INetDaemonRxApp>(GetEntitiesTypeName(domain), true)
                .ToPublic()
                .ToPartial()
                .WithBase($"Entities<{baseClass}>");

            var entityProperty = entityIds.Select(entityId => GenerateEntityProperty(entityId, domain)).ToArray();

            return entityClass.AddMembers(entityProperty);
        }

        private static Func<string, bool> EntityIsOfDomain(string domain)
        {
            return n => n.StartsWith(domain + ".", StringComparison.InvariantCultureIgnoreCase);
        }

        private PropertyDeclarationSyntax GenerateEntityProperty(string entityId, string domain)
        {
            var entityName = EntityIdHelper.GetEntity(entityId);

            var state = GetEntityStateType(entityId);

            var propertyCode = state is null
                ? $@"{GetDomainEntityTypeName(domain)}<{GetAttributesTypeName(entityId)}> {entityName.ToNormalizedPascalCase("E_")} => new(_{GetNames<INetDaemonRxApp>().VariableName}, ""{entityId}"");"
                : $@"{GetDomainEntityTypeName(domain)}<{state.FullName}, {GetAttributesTypeName(entityId)}> {entityName.ToNormalizedPascalCase("E_")} => new(_{GetNames<INetDaemonRxApp>().VariableName}, ""{entityId}"");";

            return ParseProperty(propertyCode).ToPublic();
        }

        /// <summary>
        ///     Returns a list of domains from all entities
        /// </summary>
        /// <param name="entities">A list of entities</param>
        internal static IEnumerable<string> GetDomainsFromEntities(IEnumerable<string> entities) => entities.Select(EntityIdHelper.GetDomain).Distinct();
    }
}