using System;
using Microsoft.Extensions.Logging;
using NetDaemon.Common.Reactive.Services;

namespace NetDaemon.Common.Reactive
{
    /// <summary>
    ///     Implements the observable interface for state changes
    /// </summary>
    public class StateChangeObservable : ObservableBase<StateChange>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="app">App being tracked</param>
        public StateChangeObservable(ILogger logger, INetDaemonAppBase app)
            : base(logger, app)
        { }
    }

    /// <summary>
    ///     Implements the observable interface for event changes
    /// </summary>
    public class EventObservable : ObservableBase<RxEvent>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger">Logger to use</param>
        /// <param name="app">App being tracked</param>
        public EventObservable(ILogger logger, INetDaemonAppBase app)
            : base(logger, app)
        { }
    }

    /// <summary>
    ///     Implements Observable RxEvent
    /// </summary>
    public class ReactiveEvent : IRxEvent
    {
        private readonly NetDaemonRxApp _daemonRxApp;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon host object</param>
        public ReactiveEvent(NetDaemonRxApp daemon)
        {
            _daemonRxApp = daemon;
        }

        /// <summary>
        ///     Implements IObservable ReactiveEvent
        /// </summary>
        /// <param name="observer">Observer</param>
        public IDisposable Subscribe(IObserver<RxEvent> observer)
        {
            return _daemonRxApp!.EventChangesObservable.Subscribe(observer);
        }
    }

    /// <summary>
    ///     Implements the IObservable state changes
    /// </summary>
    public class ReactiveState : IObservable<StateChange>
    {
        private readonly NetDaemonRxApp _daemonRxApp;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="daemon">The NetDaemon host object</param>
        public ReactiveState(NetDaemonRxApp daemon)
        {
            _daemonRxApp = daemon;
        }

        /// <summary>
        ///     Implements IObservable ReactivState
        /// </summary>
        /// <param name="observer">Observer</param>
        public IDisposable Subscribe(IObserver<StateChange> observer)
        {
            return _daemonRxApp.StateChangesObservable.Subscribe(observer);
        }
    }
}