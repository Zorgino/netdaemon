using System;
using System.Collections.Generic;
using System.Linq;
using NetDaemon.Common.Reactive;
using NetDaemon.Common.Reactive.Services;

namespace NetDaemon.Generated.Reactive.Services
{
    public class Entities<TAttributes> : RxEntityBase<TAttributes>
        where TAttributes : class
    {
        private readonly INetDaemonRxApp _netDaemonRxApp;
        public Entities(INetDaemonRxApp netDaemonRxApp, params string[] entityIds) : base(netDaemonRxApp, entityIds)
        {
            _netDaemonRxApp = netDaemonRxApp;
        }

        // public T Where(Func<T, bool> selector)
        // {
        //     var entityIds = GetType()
        //         .GetProperties()
        //         .Select(x => x.GetValue(this))
        //         .Cast<RxEntityBase>()
        //         .Select(e => e.EntityId)
        //         .Select(id => Activator.CreateInstance(typeof(T), _netDaemonRxApp, id))
        //         .Cast<T>()
        //         .Where(selector)
        //         .Cast<RxEntityBase>()
        //         .Select(x => x.EntityId);
        //
        //     return (T)Activator.CreateInstance(typeof(T), _netDaemonRxApp, entityIds)!;
        // }
        //
        // public static implicit operator T(Entities<T> entities)
        // {
        //     var entityIds = entities.GetEntityIds();
        //
        //     return (T)Activator.CreateInstance(typeof(T), entities._netDaemonRxApp, entityIds)!;
        // }
        //
        // private IEnumerable<string> GetEntityIds()
        // {
        //     return GetType()
        //         .GetProperties()
        //         .Select(x => x.GetValue(this))
        //         .Cast<RxEntityBase>()
        //         .Select(e => e.EntityId);
        // }
    }
}