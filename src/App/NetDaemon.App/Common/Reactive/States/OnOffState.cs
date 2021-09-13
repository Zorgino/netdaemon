namespace NetDaemon.Common.Reactive.States
{
    public class OnOffState : StateValueObject<bool?>
    {
        public OnOffState(object? state) : base(state)
        {
        }

        public virtual bool IsOn => Value == true;

        public virtual bool IsOff => Value == false;

        protected override bool? ConvertToValue()
        {
            return State switch{
                "on" => true,
                "off" => false,
                _ => null
            };
        }

        public override bool IsMissing => base.IsMissing || Value is null;

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