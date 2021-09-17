using System.Collections.Generic;
using System.Linq;
using NetDaemon.Common.Reactive.Services;

namespace NetDaemon.Common
{
    public record Target
    {
        public Target()
        {
        }

        public Target(RxEntityBase entity)
        {
            EntityIds = entity.EntityIds.ToList();
        }

        public Target(params string[] entityIds)
        {
            EntityIds = entityIds.ToList();
        }

        public Target(IEnumerable<string> entityIds)
        {
            EntityIds = entityIds.ToList();
        }

        public IReadOnlyCollection<string>? EntityIds { get; init; } = new List<string>();

        public IReadOnlyCollection<string>? DeviceIds { get; init; } = new List<string>();

        public IReadOnlyCollection<string>? AreaIds { get; init; } = new List<string>();
    }
}