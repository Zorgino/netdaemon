using System;
namespace NetDaemon.Common.Reactive.States
{
    public class ClimateState : OnOffState
    {
        private ClimateStates _state = ClimateStates.Off;

        public ClimateState(string? state) : base(state)
        {
            if (!IsMissing)
            {
                _state = state switch
                {
                    "heat" or "heating" => ClimateStates.Heat,
                    "cool" or "cooling" => ClimateStates.Cool,
                    "fan" => ClimateStates.Fan,
                    _ => ClimateStates.Off
                };
            }
        }

        public bool IsHeating => _state is ClimateStates.Heat;

        public bool IsCooling => _state is ClimateStates.Cool;

        public bool IsFan => _state is ClimateStates.Fan;

        protected override bool? ConvertFrom(string state)
        {
            return _state is not ClimateStates.Off;
        }
    }

    internal enum ClimateStates
    {
        Off,
        Heat,
        Cool,
        Fan
    }
}