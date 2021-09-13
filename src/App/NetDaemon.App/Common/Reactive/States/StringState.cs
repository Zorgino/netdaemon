using System;
namespace NetDaemon.Common.Reactive.States
{
    public class StringState : StateObject
    {
        public StringState(string? state) : base(state)
        {
        }

        public static implicit operator string?(StringState value)
        {
            return value.State;
        }

        public string? Value => Missing ? null : Value;
    }
}