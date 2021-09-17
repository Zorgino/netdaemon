using System;
namespace NetDaemon.Common.Reactive.States
{
    public abstract class StateObject
    {
        protected string? State { get; }

        protected StateObject(string? state)
        {
            State = state?.ToLowerInvariant();
        }

        public virtual bool Missing => State is null || Unavailable || Unknown;

        public bool Unavailable => State is "unavailable";

        public bool Unknown => State is "unknown";

        public override string ToString()
        {
            return State;
        }
    }
}