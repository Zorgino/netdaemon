namespace NetDaemon.Common.Reactive.States
{
    public class NumericState : ValueObject<double?>
    {
        public NumericState(string? state) : base(state)
        {
        }
    }
}