namespace NetDaemon.Common.Reactive.States
{
    public sealed class NumericState : StateObject
    {
        public NumericState(string? state) : base(state)
        {
        }

        public double? Value
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

        public override bool Missing => base.Missing || Value is null;

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