using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon.Config;
using NetDaemon.Service.App.CodeGeneration.Extensions;
using NetDaemon.Service.App.CodeGeneration.Helpers;
using static NetDaemon.Service.App.CodeGeneration.Helpers.NamingHelper;
using static NetDaemon.Service.App.CodeGeneration.Helpers.SyntaxFactoryHelper;
namespace NetDaemon.Service.App.CodeGeneration
{
    public partial class NewCodeGenerator
    {
        private static IEnumerable<TypeDeclarationSyntax> GenerateServiceTypes(IEnumerable<HassServiceDomain> serviceDomains)
        {
            var domains = serviceDomains.Select(x => x.Domain!);

            yield return GenerateRootServicesInterface(domains);

            yield return GenerateRootServicesType(domains);

            foreach (var domainServicesGroup in serviceDomains.Where(sd => sd.Services?.Any() == true).GroupBy(x => x.Domain, x => x.Services))
            {
                var domain = domainServicesGroup.Key!;
                var domainServices = domainServicesGroup
                    .SelectMany(services => services!)
                    .Select(group => group)
                    .OrderBy(x => x.Service)
                    .ToList();

                yield return GenerateServicesDomainType(domain, domainServices);

                foreach (var domainService in domainServices)
                {
                    foreach (var serviceArgsRecord in GenerateServiceArgsRecord(domain, domainService))
                    {
                        yield return serviceArgsRecord;
                    }
                }
            }
        }

        private static TypeDeclarationSyntax GenerateRootServicesType(IEnumerable<string> domains)
        {
            var haContextNames = GetNames<INetDaemonRxApp>();
            var properties = domains.Select(domain =>
            {
                var propertyCode = $"{GetServicesTypeName(domain)} {domain.ToPascalCase()} => new(_{haContextNames.VariableName});";

                return ParseProperty(propertyCode).ToPublic();
            }).ToArray();

            return ClassWithInjected<INetDaemonRxApp>("Services").WithBase((string)"IServices").AddMembers(properties).ToPublic().ToPartial();
        }

        private static TypeDeclarationSyntax GenerateRootServicesInterface(IEnumerable<string> domains)
        {
            var properties = domains.Select(domain =>
            {
                var typeName = GetServicesTypeName(domain);
                var domainName = domain.ToPascalCase();

                return Property(typeName, domainName, set: false);
            }).ToArray();

            return Interface("IServices").AddMembers(properties).ToPublic().ToPartial();
        }

        private static TypeDeclarationSyntax GenerateServicesDomainType(string domain, IEnumerable<HassService> services)
        {
            var serviceTypeDeclaration = ClassWithInjected<INetDaemonRxApp>(GetServicesTypeName(domain)).ToPublic().ToPartial();

            var serviceMethodDeclarations = services.SelectMany(service => GenerateServiceMethod(domain, service)).ToArray();

            return serviceTypeDeclaration.AddMembers(serviceMethodDeclarations);
        }

        private static IEnumerable<TypeDeclarationSyntax> GenerateServiceArgsRecord(string domain, HassService service)
        {
            var serviceArguments = GetServiceArguments(domain, service);

            if (serviceArguments is null)
            {
                yield break;
            }

            var autoProperties = serviceArguments.Arguments
                .Select(argument
                    => Property(argument.TypeName!, argument.PropertyName!)
                        .WithAttribute<JsonPropertyNameAttribute>(argument.HaName)
                        .ToPublic())
                .ToArray();

            yield return Record(serviceArguments.TypeName, autoProperties).ToPublic().ToPartial();
        }

        private static IEnumerable<MemberDeclarationSyntax> GenerateServiceMethod(string domain, HassService service)
        {
            var serviceName = service.Service!;

            var args = GetServiceArguments(domain, service);
            var fieldVar = GetVariableName<INetDaemonRxApp>("_");

            var dataVar = args.GetParametersVariable();
            var argsParametersString = args.HasArguments ? $"{args.TypeName} {(args.HasRequiredArguments ? dataVar : $"? {dataVar} = null")}" : null ;

            var serviceMethodName = GetServiceMethodName(serviceName);

            if (service.Target is not null)
            {
                var (targetClass, targetVar) = GetNames<Target>();

                if (args.HasArguments)
                {
                    yield return ParseMethod(
                        $@"void {serviceMethodName}({targetClass} {targetVar}, {argsParametersString})
                            {{
                                {fieldVar}.{nameof(INetDaemonRxApp.CallService)}(""{domain}"", ""{serviceName}"", {targetVar}, {dataVar});
                            }}").ToPublic();

                    if (!args.ContainIllegalHaName)
                    {
                        yield return ParseMethod(
                            $@"void {serviceMethodName}({targetClass} {targetVar}, {args?.GetParametersDecomposedString()})
                                {{
                                    {fieldVar}.{nameof(INetDaemonRxApp.CallService)}(""{domain}"", ""{serviceName}"", {targetVar}, {args?.GetParametersDecomposedVariable()});
                                }}").ToPublic();
                    }
                }
                else
                {
                    yield return ParseMethod(
                        $@"void {serviceMethodName}({targetClass} {targetVar})
                            {{
                                {fieldVar}.{nameof(INetDaemonRxApp.CallService)}(""{domain}"", ""{serviceName}"", {targetVar});
                            }}").ToPublic();
                }
            }
            else
            {
                if (args!.HasArguments)
                {
                    yield return ParseMethod(
                        $@"void {serviceMethodName}({argsParametersString})
                                {{
                                    {fieldVar}.{nameof(INetDaemonRxApp.CallService)}(""{domain}"", ""{serviceName}"", null, {dataVar});
                                }}").ToPublic();

                    if (!args.ContainIllegalHaName)
                    {
                        yield return ParseMethod(
                            $@"void {serviceMethodName}({args.GetParametersDecomposedString()})
                                {{
                                    {fieldVar}.{nameof(INetDaemonRxApp.CallService)}(""{domain}"", ""{serviceName}"", null, {args.GetParametersDecomposedVariable()});
                                }}").ToPublic();
                    }
                }
            }
        }

        private static ServiceArguments GetServiceArguments(string domain, HassService service)
        {
            return new(domain, service.Service!, service.Fields);
        }
    }
}