namespace NetDaemon.Common.Reactive.States
{
    public sealed class NumericState : StateObject<double?>
    {
        public NumericState(string? state) : base(state)
        {
        }

        public override double? Value
        {
            get
            {
                if (double.TryParse(State, out var result))
                {
                    return result;
                }

                return null;
            }
        }

        public override bool IsMissing => base.IsMissing || Value is null;

        public static implicit operator double(NumericState state)
        {
            return state.Value ?? default;
        }

        public static implicit operator double?(NumericState state)
        {
            return state.Value;
        }
    }
}