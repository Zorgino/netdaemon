using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetDaemon.Common.Reactive;
using NetDaemon.Common.Reactive.Services;
namespace NetDaemon.Common
{
    public interface INetDaemonApp : INetDaemonRxApp, IApplicationMetadata, INetDaemonPersistantApp
    {
        /// <summary>
        ///     Returns the observables events implementation of AppDaemonRxApps
        /// </summary>
        ObservableBase<RxEvent> EventChangesObservable { get; }
        /// <summary>
        ///     Returns the observables states implementation of AppDaemonRxApps
        /// </summary>
        ObservableBase<StateChange> StateChangesObservable { get; }

        IList<(string, string, Func<dynamic?, Task>)> DaemonCallBacksForServiceCalls { get; }
    }
}