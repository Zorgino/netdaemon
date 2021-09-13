using System;
namespace NetDaemon.Common.Reactive.States
{
    public class StringState : StateValueObject<string>
    {
        public StringState(string? state) : base(state)
        {
        }

        public static implicit operator string?(StringState value)
        {
            return value.State;
        }
    }
}