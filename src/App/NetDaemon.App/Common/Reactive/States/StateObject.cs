using System;
namespace NetDaemon.Common.Reactive.States
{
    public abstract class StateObject
    {
        protected string? State { get; }

        protected StateObject(string? state)
        {
            State = state;
        }

        public virtual bool IsMissing => State is null || IsUnavailable || IsUnknown;

        public bool IsUnavailable => State is "unavailable";

        public bool IsUnknown => State is "unknown";
    }
}