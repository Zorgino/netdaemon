﻿using System;
using System.Collections.Generic;
using System.Linq;
using JoySoftware.HomeAssistant.Model;
using Service.CodeGenerator.Extensions;
using static Service.CodeGenerator.Helpers.NamingHelper;

namespace Service.CodeGenerator
{
    internal record ServiceArgument
    {
        public Type? Type { get; init; }

        public bool Required { get; init; }

        public string? HaName { get; init; }

        public string? TypeName => Type?.GetFriendlyName();

        public string? ParameterTypeName => Required ? TypeName : $"{TypeName}?";

        public string? PropertyName => HaName?.ToNormalizedPascalCase();

        public string? VariableName => HaName?.ToNormalizedCamelCase();

        public string? ParameterVariableName => Required ? VariableName : $"{VariableName} = null";
    }

    internal class ServiceArguments
    {
        private readonly string _serviceName;
        private readonly string _domain;

        public ServiceArguments(string domain, string serviceName, IReadOnlyCollection<HassServiceField> serviceFields)
        {
            _domain = domain;
            _serviceName = serviceName!;
            Arguments = serviceFields.Select(GetArgument);
        }

        public IEnumerable<ServiceArgument> Arguments { get; }

        public bool HasRequiredArguments => Arguments.Any(v => v.Required);

        public string TypeName => GetServiceArgumentsTypeName(_domain, _serviceName);

        private static ServiceArgument GetArgument(HassServiceField field)
        {
            Type type = GetTypeFromSelector(field.Selector);

            return new ServiceArgument
            {
                HaName = field.Field!,
                Type = type,
                Required = field.Required == true
            };
        }

        public string GetParametersString()
        {
            // adding {(HasRequiredArguments ? "" : "= null")} causes ambiguity in a call with optional args
            return $"{TypeName} data";
        }

        public static string GetParametersVariable()
        {
            return "data";
        }

        public string GetParametersDecomposedString()
        {
            var argumentList = Arguments.OrderByDescending(arg => arg.Required);

            var anonymousVariableStr = argumentList.Select(x => $"{x.ParameterTypeName} @{x.ParameterVariableName}");

            return $"{string.Join(", ", anonymousVariableStr)}";
        }

        public string GetParametersDecomposedVariable()
        {
            var anonymousVariableStr = Arguments.Select(x => $"@{x.HaName} = @{x.VariableName}");

            return $"new {{ {string.Join(", ", anonymousVariableStr)} }}";
        }

        private static Type GetTypeFromSelector(object? selectorObject)
        {
            return selectorObject switch
            {
                ActionSelector
                    or AreaSelector
                    or AddonSelector
                    or EntitySelector
                    or DeviceSelector
                    or ObjectSelector
                    or TargetSelector
                    or TextSelector
                    or null => typeof(string),
                BooleanSelector => typeof(bool),
                NumberSelector => typeof(long),
                TimeSelector => typeof(DateTime),
                SelectSelector => typeof(List<string>),
                _ => throw new ArgumentOutOfRangeException(nameof(selectorObject), selectorObject, null)
            };
        }
    }
}