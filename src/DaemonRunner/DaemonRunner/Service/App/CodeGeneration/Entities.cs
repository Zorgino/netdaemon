using System;
using System.Collections.Generic;
using System.Linq;
using NetDaemon.Common.Reactive;
using NetDaemon.Common.Reactive.Services;

namespace NetDaemon.Generated.Reactive.Services
{
    public class Entities<T>
        where T: class
    {
        private readonly INetDaemonRxApp _netDaemonRxApp;
        public Entities(INetDaemonRxApp netDaemonRxApp)
        {
            _netDaemonRxApp = netDaemonRxApp;
        }

        public IEnumerable<T> Where(Func<T, bool> selector)
        {
            return GetType()
                .GetProperties()
                .Select(x => x.GetValue(this))
                .Cast<RxEntityBase>()
                .Select(e => e.EntityId)
                .Select(id => Activator.CreateInstance(typeof(T), _netDaemonRxApp, id))
                .Cast<T>()
                .Where(selector);
        }
    }
}