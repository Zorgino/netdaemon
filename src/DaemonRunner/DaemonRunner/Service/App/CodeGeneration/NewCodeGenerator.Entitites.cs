using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            var entityDomains = entityIds.Select(EntityHelper.GetDomain).Distinct().OrderBy(s => s).ToList();

            return new[]
            {
                new []
                {
                    GenerateRootEntitiesInterface(entityDomains),
                    GenerateRootEntitiesClass(entityDomains)
                },

                GenerateDomainEntitiesTypes(entities),
                GenerateEntityDomainBaseTypes(entityDomains),
                GenerateEntityDomainAttributeRecords(entities),
            }.SelectMany(x => x);
        }

        #region Entities : IEntities
        private static TypeDeclarationSyntax GenerateRootEntitiesInterface(IEnumerable<string> domains)
        {
            var autoProperties = domains.Select(domain =>
            {
                var typeName = GetDomainEntitiesTypeName(domain);
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
                var entitiesTypeName = GetDomainEntitiesTypeName(domain);
                var entitiesPropertyName = domain.ToPascalCase();

                return ParseProperty($"{entitiesTypeName} {entitiesPropertyName} => new(_{haContextNames.VariableName});").ToPublic();
            }).ToArray();

            return ClassWithInjected<INetDaemonRxApp>("Entities").WithBase((string)"IEntities")
                .AddMembers(properties)
                .ToPublic()
                .ToPartial();
        }
        #endregion

        #region ClimateEntity, ClimateEntity<TAttributes>, ClimateEntity<TState, TAttributes>

        private IEnumerable<TypeDeclarationSyntax> GenerateEntityDomainBaseTypes(IEnumerable<string> domains)
        {
            return domains.SelectMany(GenerateEntityDomainBaseTypes);
        }

        private IEnumerable<TypeDeclarationSyntax> GenerateEntityDomainBaseTypes(string domain)
        {
            return new[]
            {
                GenerateEntityDomainGenericStateAndAttributesType(domain),
                GenerateEntityDomainGenericStateType(domain),
                GenerateEntityDomainCommonType(domain),
            }
                .Select(x => x.ToPublic().ToPartial());
        }

        private static TypeDeclarationSyntax GenerateEntityDomainGenericStateAndAttributesType(string domain)
        {
            var (className, variableName) = GetNames<INetDaemonRxApp>();

            var entityClass = GetDomainEntityTypeName(domain);
            var entityGenericClass = $"{entityClass}<{TState}, {TAttributes}>";

            string baseClass = $"{typeof(RxEntityBase).FullName}<{entityGenericClass}, {typeof(EntityState).FullName}<{TState}, {TAttributes}>, {TState}, {TAttributes}>";
            string classDeclaration = $@"class {entityGenericClass} : {baseClass}
                                            {GetReferenceTypeConstraint(TAttributes)}
                                            {GetTypeConstraint(TState, typeof(StateObject))}
                                    {{
                                            public {entityClass}({className} {variableName}, params string[] entityIds) : base({variableName}, entityIds)
                                            {{
                                            }}
                                    }}";

            return ParseClass(classDeclaration);
        }

        private static TypeDeclarationSyntax GenerateEntityDomainGenericStateType(string domain)
        {
            var (className, variableName) = GetNames<INetDaemonRxApp>();

            var entityClass = GetDomainEntityTypeName(domain);
            var attributesTypeName = GetAttributesTypeName(domain);

            var entityGenericClass = $"{entityClass}<{TState}>";
            string baseClass = $"{typeof(RxEntityBase).FullName}<{entityGenericClass}, {typeof(EntityState).FullName}<{TState}, {attributesTypeName}>, {TState}, {attributesTypeName}>";

            string classDeclaration = $@"class {entityGenericClass} : {baseClass}
                                            {GetReferenceTypeConstraint(TState)}
                                    {{
                                            public {entityClass}({className} {variableName}, params string[] entityIds) : base({variableName}, entityIds)
                                            {{
                                            }}
                                    }}";

            return ParseClass(classDeclaration);
        }

        private TypeDeclarationSyntax GenerateEntityDomainCommonType(string domain)
        {
            var (className, variableName) = GetNames<INetDaemonRxApp>();

            var entityClass = GetDomainEntityTypeName(domain);

            var stateType = GetDomainEntityStateType(domain) ?? typeof(StringState);
            var baseClass = $"{entityClass}<{stateType}>";

            var classDeclaration = $@"class {entityClass} : {baseClass}
                                    {{
                                            public {entityClass}({className} {variableName}, params string[] entityIds) : base({variableName}, entityIds)
                                            {{
                                            }}
                                    }}";

            return ParseClass(classDeclaration);
        }

        #endregion

        #region ClimateAttributes, ClimateAcAttributes

        private IEnumerable<HaAttributeProperty> GetExistingProperties(string attributesTypeName)
        {
            var partialEntityType = GetType()
                .Assembly
                .DefinedTypes
                .FirstOrDefault(t => t.Name == attributesTypeName);

            if (partialEntityType is null)
            {
                return Array.Empty<HaAttributeProperty>();
            }

            return partialEntityType
                .DeclaredProperties
                .Where(x => x.PropertyType == typeof(JsonElement))
                .Select(p
                    => new
                    {
                        HaName = p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name,
                    })
                .Where(x => x.HaName is not null)
                .Select(x =>
                    new{
                        x.HaName,
                        partialEntityType
                                .DeclaredProperties
                                .FirstOrDefault(p => p.PropertyType != typeof(JsonElement) && p.Name == HaAttributeProperty.GetPropName(x.HaName))?
                                .PropertyType
                        })
                .Where(x => x.PropertyType is not null)
                .Select(p
                    => new
                    {
                        p.HaName,
                        Type = p.PropertyType!
                    })
                .Select(x => new HaAttributeProperty(x.HaName!, x.Type));
        }

        private IEnumerable<TypeDeclarationSyntax> GenerateEntityDomainAttributeRecords(IEnumerable<IEntityProperties> entities)
        {
            var mainRecords = new List<TypeDeclarationSyntax>();
            var partialCommentedRecords = new List<TypeDeclarationSyntax>();

            foreach (var entityDomainGroups in entities.GroupBy(x => EntityHelper.GetDomain(x.EntityId)))
            {
                var attributesTypeName = GetAttributesTypeName(entityDomainGroups.Key);

                var attrs = entityDomainGroups
                    .Select(es => new Dictionary<string, object>(es.Attribute))
                    .SelectMany(x => x)
                    .Select(x => new HaAttributeProperty(x.Key, x.Value.GetTypeByValues().ToTypeCanBeImplicitlyConvertedTo()))
                    .Concat(GetExistingProperties(attributesTypeName))
                    .OrderBy(a => a.HaName)
                    .Distinct()
                    .ToList();

                var conflictingHaAttributes = GetConflictingEntityAttributes(attrs).ToList();

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

        private static IEnumerable<HaAttributeProperty> GetConflictingEntityAttributes(IEnumerable<HaAttributeProperty> attrs)
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

        #endregion

        #region ClimateEntities

        private IEnumerable<TypeDeclarationSyntax> GenerateDomainEntitiesTypes(IEnumerable<IEntityProperties> entities)
        {
            return entities.GroupBy(EntityHelper.GetDomain, GenerateDomainEntitiesType);
        }

        private TypeDeclarationSyntax GenerateDomainEntitiesType(string domain, IEnumerable<IEntityProperties> entities)
        {
            var entityDomainTypeName = GetDomainEntitiesTypeName(domain);

            var entityClass = ClassWithInjected<INetDaemonRxApp>(entityDomainTypeName, true, entities.Select(x => x.EntityId))
                .ToPublic()
                .ToPartial()
                .WithBase(GetDomainEntityTypeName(domain));

            var entityProperty = entities
                .Where(EntityIsNotNetDaemonGenerated)
                .Select(x => x.EntityId)
                .Select(GenerateEntityProperty)
                .ToArray();

            return entityClass.AddMembers(entityProperty);
        }

        private bool EntityIsNotNetDaemonGenerated(IEntityProperties entity)
        {
            return entity.Attribute.integration != "netdaemon";
        }

        private PropertyDeclarationSyntax GenerateEntityProperty(string entityId)
        {
            var domainEntityTypeName = GetDomainEntityTypeName(EntityHelper.GetDomain(entityId));
            // var attributesTypeName = GetAttributesTypeName(entityId);
            var state = GetEntityStateType(entityId);
            var propertyName = EntityHelper.GetEntity(entityId).ToNormalizedPascalCase("E_");
            var variableName = GetNames<INetDaemonRxApp>().VariableName;

            var propertyCode = state is null
                ? $@"{domainEntityTypeName} {propertyName} => new(_{variableName}, ""{entityId}"");"
                : $@"{domainEntityTypeName}<{state.FullName}> {propertyName} => new(_{variableName}, ""{entityId}"");";

            return ParseProperty(propertyCode).ToPublic();
        }

        #endregion
    }
}