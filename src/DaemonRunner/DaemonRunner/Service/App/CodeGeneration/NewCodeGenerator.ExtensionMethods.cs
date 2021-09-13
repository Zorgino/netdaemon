using System.Collections.Generic;
using System.Linq;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Common.Reactive.Services;
using NetDaemon.Service.App.CodeGeneration.Helpers;
using static NetDaemon.Service.App.CodeGeneration.Helpers.NamingHelper;
using static NetDaemon.Service.App.CodeGeneration.Helpers.SyntaxFactoryHelper;
using OldEntityState = NetDaemon.Common.EntityState;

namespace NetDaemon.Service.App.CodeGeneration
{
    public partial class NewCodeGenerator
    {
        private static IEnumerable<ClassDeclarationSyntax> GenerateExtensionMethodClasses(IEnumerable<HassServiceDomain> serviceDomains, IEnumerable<OldEntityState> entities)
        {
            var entityDomains = entities.GroupBy(e => EntityIdHelper.GetDomain(e.EntityId)).Select(x => x.Key);

            foreach (var domainServicesGroup in serviceDomains
                .Where(sd =>
                    sd.Services?.Any() == true
                    && sd.Services.Any(s => entityDomains.Contains(s.Target?.Entity?.Domain)))
                .GroupBy(x => x.Domain, x => x.Services))
            {
                var domain = domainServicesGroup.Key!;
                var domainServices = domainServicesGroup
                    .SelectMany(services => services!)
                    .Where(s => s.Target?.Entity?.Domain != null)
                    .Select(group => group)
                    .OrderBy(x => x.Service)
                    .ToList();

                yield return GenerateDomainExtensionClass(domain, domainServices);
            }
        }

        private static ClassDeclarationSyntax GenerateDomainExtensionClass(string domain, IEnumerable<HassService> services)
        {
            var serviceTypeDeclaration = Class(GetEntityDomainExtensionMethodClassName(domain)).ToPublic().ToStatic();

            var serviceMethodDeclarations = services
                .SelectMany(service => GenerateExtensionMethods(domain, service))
                .Select(ext => ext.ToPublic().ToStatic())
                .ToArray();

            return serviceTypeDeclaration.AddMembers(serviceMethodDeclarations);
        }

        private static IEnumerable<GlobalStatementSyntax> GenerateExtensionMethods(string domain, HassService service)
        {
            var serviceName = service.Service!;

            var args = GetServiceArguments(domain, service);

            var entityTypeName = GetDomainEntityTypeName(domain);
            var entityVar = "entity";

            var serviceMethodName = GetServiceMethodName(serviceName);
            var callServiceName = nameof(RxEntityBase.CallService);

            if (args.HasArguments)
            {
                yield return ParseMethod(
                    $@"void {serviceMethodName}<T>(this {entityTypeName}<T> {entityVar}, {args.GetParametersString()})
                            where T : class
                        {{
                            {entityVar}.{callServiceName}(""{serviceName}"", {args.GetParametersVariable()});
                        }}");

                if (args.ContainIllegalHaName)
                {
                    yield return ParseMethod(
                        $@"void {serviceMethodName}<T>(this {entityTypeName}<T> {entityVar} , {args.GetParametersDecomposedString()})
                            where T : class
                            {{
                                {entityVar}.{callServiceName}(""{serviceName}"", {args.GetParametersDecomposedVariable()});
                            }}");
                }
            }
            else
            {
                yield return ParseMethod(
                    $@"void {serviceMethodName}<T>(this {entityTypeName}<T> {entityVar})
                            where T : class
                        {{
                            {entityVar}.{callServiceName}(""{serviceName}"");
                        }}");
            }
        }
    }
}