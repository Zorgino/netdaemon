using System.Collections.Generic;
using System.Linq;

namespace NetDaemon.Common
{
    public record Target
    {
        public Target()
        {
        }

        public Target(params string[] entityIds)
        {
            EntityIds = entityIds.ToList();
        }

        public Target(IEnumerable<string> entityIds)
        {
            EntityIds = entityIds.ToList();
        }

        public IReadOnlyCollection<string>? EntityIds { get; init; }

        public IReadOnlyCollection<string>? DeviceIds { get; init; }

        public IReadOnlyCollection<string>? AreaIds { get; init; }
    }
}