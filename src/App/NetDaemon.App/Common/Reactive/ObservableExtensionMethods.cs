using System;
using System.Reactive.Linq;
using System.Threading;
using NetDaemon.Common.Reactive.Services;

namespace NetDaemon.Common.Reactive
{
    /// <summary>
    ///     Extension methods for Observables
    /// </summary>
    public static class ObservableExtensionMethods
    {
        // /// <summary>
        // ///     Is same for timespan time
        // /// </summary>
        // /// <param name="observable"></param>
        // /// <param name="span"></param>
        // public static IObservable<StateChange> NDSameStateFor(this IObservable<StateChange> observable, TimeSpan span)
        // {
        //     return observable.Throttle(span);
        // }
        //
        // /// <summary>
        // ///     Wait for state the specified time
        // /// </summary>
        // /// <param name="observable"></param>
        // /// <param name="timeout">Timeout waiting for state</param>
        // public static IObservable<StateChange> NDWaitForState(this IObservable<StateChange> observable, TimeSpan timeout)
        // {
        //     return observable
        //         .Timeout(timeout,
        //         Observable.Return(new StateChange(new EntityState() { State = "TimeOut" }, new EntityState() { State = "TimeOut" }))).Take(1);
        // }

        // /// <summary>
        // ///     Wait for state the default time
        // /// </summary>
        // /// <param name="observable"></param>
        // public static IObservable<StateChange> NDWaitForState(this IObservable<StateChange> observable) => observable
        //     .Timeout(TimeSpan.FromSeconds(5),
        //     Observable.Return((new EntityState() { State = "TimeOut" }, new EntityState() { State = "TimeOut" }))).Take(1);

        /// <summary>
        ///     Returns first occurence or null if timedout
        /// </summary>
        /// <param name="observable">Extended object</param>
        /// <param name="timeout">The time to wait before timeout.</param>
        /// <param name="token">Provide token to cancel early</param>
        public static StateChange? NDFirstOrTimeout(this IObservable<StateChange> observable, TimeSpan timeout, CancellationToken? token = null)
        {
            try
            {
                if (token is null)
                    return observable.Timeout(timeout).Take(1).Wait();
                else
                    return observable.Timeout(timeout).Take(1).RunAsync(token.Value).Wait();
            }
            catch (TimeoutException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }
}