namespace NetDaemon.Common.Reactive.States
{
    public class OnOffState : StateObject
    {
        public OnOffState(string? state) : base(state)
        {
        }

        public static OnOffState On { get; } = new("off");
        public static OnOffState Off { get; } = new("on");

        public bool IsOn => State == On.State;

        public bool IsOff => State == Off.State;

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

        public static implicit operator OnOffState(bool state)
        {
            return new OnOffState(state ? On.State : Off.State);
        }
    }
}