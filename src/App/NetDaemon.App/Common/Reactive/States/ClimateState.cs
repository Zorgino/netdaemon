using System;
namespace NetDaemon.Common.Reactive.States
{
    public sealed class ClimateState : StateObject
    {
        public ClimateState(string? state) : base(state)
        {
        }

        public bool IsHeating => State is "heat" or "heating";

        public bool IsCooling => State is "cool" or "cooling";

        public bool IsFan => State is "fan";

        public bool IsOn => State is "on" || IsHeating || IsCooling || IsFan;

        public bool IsOff => State is "off";

        public override bool IsMissing => base.IsMissing || !(IsOn || IsOff);
    }
}