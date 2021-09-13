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

        protected override bool? ConvertToValue()
        {
            return IsHeating || IsCooling || IsFan || State is "on";
        }
    }
}