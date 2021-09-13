using System;
namespace NetDaemon.Common.Reactive.States
{
    public sealed class ClimateState : StateObject
    {
        public ClimateState(string? state) : base(state)
        {
        }

        public bool Heating => State is "heat" or "heating";

        public bool Cooling => State is "cool" or "cooling";

        public bool Fan => State is "fan";

        public bool On => State is "on" || Heating || Cooling || Fan;

        public bool Off => State is "off";

        public override bool Missing => base.Missing || !(On || Off);
    }
}