using System;
namespace NetDaemon.Common.Reactive.States
{
    public abstract class StateValueObject<T>
    {
        private readonly Lazy<T> _lazyValue;

        protected string? State { get; }

        protected StateValueObject(string? state)
        {
            State = state;

            _lazyValue = new Lazy<T>(ConvertToValue);
        }

        protected virtual T? ConvertToValue()
        {
            try
            {
                return (T)Convert.ChangeType(State, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        public virtual bool IsMissing => State is null || IsUnavailable || IsUnknown;

        public bool IsUnavailable => State is "unavailable";

        public bool IsUnknown => State is "unknown";

        public T? Value => _lazyValue.Value;

        // public static implicit operator string(ValueObject<T> state)
        // {
        //     return state.Value?.ToString();
        // }
        //
        // public static implicit operator T?(ValueObject<T> state)
        // {
        //     return state.Value;
        // }
        //
        // public static explicit operator ValueObject<T>(string state)
        // {
        //     var value = state is not "unknown" or "unavailable" or null
        //         ? (T)Convert.ChangeType(state, typeof(T))
        //         : default;
        //
        //     return new ValueObject<T>(value);
        // }
    }
}