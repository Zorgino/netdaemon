using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using JoySoftware.HomeAssistant.Model;

namespace NetDaemon.Common.Reactive.Services
{
    /// <summary>
    ///     Base class for reactive entity management
    /// </summary>
    public  class RxEntityBase : RxEntity
    {
        /// <summary>
        /// Gets the id of the entity
        /// </summary>
        public string EntityId => EntityIds.First();

        /// <summary>
        /// Gets the entity state
        /// </summary>
        public virtual EntityState? EntityState => DaemonRxApp?.State(EntityId);

        /// <summary>
        /// Gets the Area to which an entity is assigned
        /// </summary>
        public string? Area => DaemonRxApp?.State(EntityId)?.Area;

        /// <summary>
        /// Gets the entity attribute
        /// </summary>
        public virtual dynamic? Attribute => DaemonRxApp?.State(EntityId)?.Attribute;

        /// <summary>
        /// Gets a <see cref="DateTime"/> that indicates the last time the entity's state changed
        /// </summary>
        public DateTime? LastChanged => DaemonRxApp?.State(EntityId)?.LastChanged;

        /// <summary>
        ///  Gets a <see cref="DateTime"/> that indicates the last time the entity's state updated
        /// </summary>
        public DateTime? LastUpdated => DaemonRxApp?.State(EntityId)?.LastUpdated;

        /// <summary>
        /// Gets the entity's state
        /// </summary>
        public virtual dynamic? State => DaemonRxApp?.State(EntityId)?.State;

        /// <summary>
        /// Representing an AlarmControlPanel entity.
        /// </summary>
        /// <param name="daemon">An instance of <see cref="INetDaemonRxApp"/>.</param>
        /// <param name="entityIds">A list of entity id's that represent this entity</param>
        public RxEntityBase(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        /// Performs a specified service call to a specified domain with specified data
        /// </summary>
        /// <param name="domain">The domain to which the service call belongs</param>
        /// <param name="service">The service in the domain to call</param>
        /// <param name="data">Additional data to send to the service</param>
        /// <param name="sendEntityId">If true it will include the entity_id attribute with this entity's EntityId with the service call</param>
        protected void CallService(string domain, string service, dynamic? data = null, bool sendEntityId = false)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            if (sendEntityId)
                serviceData["entity_id"] = EntityId;

            DaemonRxApp.CallService(domain, service, serviceData);
        }

        public void CallServiceTargeted(string domain, string service, dynamic? data, bool waitForResponse = false)
        {
            DaemonRxApp.CallServiceTargeted(domain, service, new HassTarget() { EntityIds = new List<string>(EntityIds)}, data);
        }
    }

    public abstract class RxEntityBase<TEntity, TEntityState, TState, TAttributes> : RxEntityBase
        where TEntity : RxEntityBase<TEntity, TEntityState, TState, TAttributes>
        where TEntityState : EntityState<TState, TAttributes>
        where TAttributes : class
        where TState : class
    {
        private readonly Lazy<TAttributes?> _attributesLazy;

        protected RxEntityBase(INetDaemonRxApp haContext, string entityId) : base(haContext, new [] { entityId })
        {
            _attributesLazy = new(() => EntityState?.AttributesJson.ToObject<TAttributes>());
        }

        // We need a 'new' here because the normal type of State is string and we cannot overload string with eg double
        // TODO: smarter conversion of string to TState to take into account 'Unavalable' etc
        public override TState? State => base.State == null ? default : (TState?)Convert.ChangeType(base.State, typeof(TState), CultureInfo.InvariantCulture);

        public override TAttributes? Attribute => _attributesLazy.Value;

        public override TEntityState? EntityState => MapNullableState(base.EntityState);

        public override IObservable<StateChange</*TEntity, */TEntityState>> StateAllChanges =>
            base.StateAllChanges.Select(e => new StateChange</*TEntity,*/ TEntityState>(/*(TEntity)this,*/ MapNullableState(e.Old), MapNullableState(e.New)));

        public override IObservable<StateChange</*TEntity,*/ TEntityState>> StateChanges =>
            base.StateChanges.Select(e => new StateChange</*TEntity,*/ TEntityState>(/*(TEntity)this,*/ MapNullableState(e.Old), MapNullableState(e.New)));

        private static TEntityState? MapNullableState(EntityState? state)
        {
            // TODO: this requires the TEntityState to have a copy ctor from EntityState,
            // maybe we could make this work without the copy ctor
            return state == null ? null : (TEntityState)Activator.CreateInstance(typeof(TEntityState), state)!;
        }
    }

}