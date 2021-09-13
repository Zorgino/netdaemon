using System;
namespace NetDaemon.Common.Reactive.States
{
    public abstract class ValueObject<T>
    {
        private readonly string? _state;

        protected ValueObject(string? state)
        {
            _state = state;
        }

        protected virtual T? ConvertFrom(string state)
        {
            try
            {
                return (T)Convert.ChangeType(state, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        public virtual bool IsMissing => _state is null or "unavailable" or "unknown";

        public T? Value => IsMissing ? default : ConvertFrom(_state);

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