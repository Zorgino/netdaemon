using System;
namespace NetDaemon.Common.Reactive.States
{
    public class StringState : StateObject<string?>
    {
        public StringState(string? state) : base(state)
        {
        }

        public static implicit operator string?(StringState value)
        {
            return value.State;
        }

        public override string? Value => IsMissing ? null : Value;
    }
}