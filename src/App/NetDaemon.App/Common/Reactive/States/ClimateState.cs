using System;
namespace NetDaemon.Common.Reactive.States
{
    public class ClimateState : OnOffState
    {
        public ClimateState(string? state) : base(state)
        {
        }

        public bool IsHeating => State is "heat" or "heating";

        public bool IsCooling => State is "cool" or "cooling";

        public bool IsFan => State is "fan";

        public override bool IsOn => base.IsOn || IsCooling || IsHeating || IsFan;

        public override bool IsMissing => base.IsMissing || !IsOn;
    }
}