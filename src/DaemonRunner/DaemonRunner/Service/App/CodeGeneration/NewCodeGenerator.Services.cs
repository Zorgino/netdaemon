﻿using System.Collections.Generic;
using System.Linq;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon.Config;
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

            return ClassWithInjected<INetDaemonRxApp>("Services").WithBase((string)"IServices").AddMembers(properties).ToPublic();
        }

        private static TypeDeclarationSyntax GenerateRootServicesInterface(IEnumerable<string> domains)
        {
            var properties = domains.Select(domain =>
            {
                var typeName = GetServicesTypeName(domain);
                var domainName = domain.ToPascalCase();

                return Property(typeName, domainName, set: false);
            }).ToArray();

            return Interface("IServices").AddMembers(properties).ToPublic();
        }

        private static TypeDeclarationSyntax GenerateServicesDomainType(string domain, IEnumerable<HassService> services)
        {
            var serviceTypeDeclaration = ClassWithInjected<INetDaemonRxApp>(GetServicesTypeName(domain)).ToPublic();

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
                .Select(argument => Property(argument.TypeName!, argument.PropertyName!).ToPublic())
                .ToArray();

            yield return Record(serviceArguments.TypeName, autoProperties).ToPublic();
        }

        private static IEnumerable<MemberDeclarationSyntax> GenerateServiceMethod(string domain, HassService service)
        {
            var serviceName = service.Service!;

            var serviceArguments = GetServiceArguments(domain, service);
            var haContextVariableName = GetVariableName<INetDaemonRxApp>("_");

            var argsParametersString = serviceArguments is not null ? $"{serviceArguments.TypeName} data {(serviceArguments.HasRequiredArguments ? "" : "= null")}" : null ;

            if (service.Target is not null)
            {
                yield return ParseMethod(
                    $@"void {GetServiceMethodName(serviceName)}({typeof(Target).FullName} target {(argsParametersString is not null ? "," : "")} {argsParametersString})
                {{
                    {haContextVariableName}.{nameof(INetDaemonRxApp.CallService)}(""{domain}"", ""{serviceName}"", target {(serviceArguments is not null ? ", data" : string.Empty)});
                }}").ToPublic();
            }
            else
            {
                yield return ParseMethod(
                    $@"void {GetServiceMethodName(serviceName)}({argsParametersString})
                {{
                    {haContextVariableName}.{nameof(INetDaemonRxApp.CallService)}(""{domain}"", ""{serviceName}"" {(serviceArguments is not null ? ", null" : "")} {(serviceArguments is not null ? ", data" : "")});
                }}").ToPublic();
            }
        }

        private static ServiceArguments? GetServiceArguments(string domain, HassService service)
        {
            if (service.Fields is null || service.Fields.Count == 0)
            {
                return null;
            }

            return new ServiceArguments(domain, service.Service!, service.Fields);
        }
    }
}