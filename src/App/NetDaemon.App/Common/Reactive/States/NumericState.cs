namespace NetDaemon.Common.Reactive.States
{
    public class NumericState : StateValueObject<double?>
    {
        public NumericState(string? state) : base(state)
        {
        }

        // protected double? Value
        // {
        //     get
        //     {
        //         if (double.TryParse(State, out var result))
        //         {
        //             return null;
        //         }
        //
        //         return result;
        //     }
        // }

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