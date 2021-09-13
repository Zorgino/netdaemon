namespace NetDaemon.Common.Reactive.States
{
    public class OnOffState : StateObject<bool?>
    {
        public OnOffState(string? state) : base(state)
        {
        }

        public bool IsOn => Value == true;

        public bool IsOff => Value == false;

        public override bool IsMissing => base.IsMissing || Value is null;

        public override bool? Value => State switch{
            "on" => true,
            "off" => false,
            _ => null
        };

        public static implicit operator bool(OnOffState state)
        {
            return !state.IsMissing && state.IsOn;
        }

        public static implicit operator bool?(OnOffState state)
        {
            return state.Value;
        }
    }
}