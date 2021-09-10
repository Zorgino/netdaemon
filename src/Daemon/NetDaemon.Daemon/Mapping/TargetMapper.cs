using System;
using System.Collections.Generic;
using System.Text.Json;
using JoySoftware.HomeAssistant.Model;
using NetDaemon.Common;
using NetDaemon.Infrastructure.Extensions;
namespace NetDaemon.Mapping
{
    public static class TargetMapper
    {
        public static HassTarget? Map(this Target? target)
        {
            if (target is null) return null;

            return new HassTarget
            {
                EntityIds = target.EntityIds,
                AreaIds = target.AreaIds,
                DeviceIds = target.DeviceIds
            };
        }
    }
}