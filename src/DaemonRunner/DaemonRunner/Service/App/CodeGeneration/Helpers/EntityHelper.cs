using System;
using NetDaemon.Common;
namespace NetDaemon.Service.App.CodeGeneration.Helpers
{
    public static class EntityHelper
    {
        public static string GetDomain(string str)
        {
            return str[..str.IndexOf(".", StringComparison.InvariantCultureIgnoreCase)];
        }

        public static string GetDomain(IEntityProperties entityProperties)
        {
            return GetDomain(entityProperties.EntityId);
        }

        public static string GetEntity(string str)
        {
            return str[(str.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1)..];
        }
    }
}