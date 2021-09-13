namespace NetDaemon.Common.Reactive.States
{
    public class OnOffState : StateObject
    {
        public OnOffState(string? state) : base(state)
        {
        }

        public bool IsOn => State is "on";

        public bool IsOff => State is "off";

        public bool? Value => State switch{
            "on" => true,
            "off" => false,
            _ => null
        };

        public override bool Missing => base.Missing || Value is null;

        public static implicit operator bool(OnOffState state)
        {
            return !state.Missing && state.IsOn;
        }

        public static implicit operator bool?(OnOffState state)
        {
            return state.Value;
        }
    }
}