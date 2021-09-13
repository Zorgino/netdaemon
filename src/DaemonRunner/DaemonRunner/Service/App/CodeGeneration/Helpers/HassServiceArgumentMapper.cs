using System;
using System.Collections.Generic;
using JoySoftware.HomeAssistant.Model;
namespace NetDaemon.Service.App.CodeGeneration.Helpers
{
    internal static class HassServiceArgumentMapper
    {
        public static ServiceArgument Map(HassServiceField field)
        {
            Type type = GetTypeFromSelector(field.Selector);

            return new ServiceArgument
            {
                HaName = field.Field!,
                Type = type,
                Required = field.Required == true
            };
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
                    or SelectSelector
                    or TargetSelector
                    or TextSelector
                    or null => typeof(string),
                BooleanSelector => typeof(bool),
                NumberSelector => typeof(long),
                ObjectSelector => typeof(object),
                TimeSelector => typeof(DateTime),
                _ => typeof(string)
            };
        }
    }
}