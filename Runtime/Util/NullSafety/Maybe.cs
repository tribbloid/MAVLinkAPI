#nullable enable
using System;
using System.Threading;

namespace MAVLinkAPI.Util.NullSafety
{
    public struct Maybe<T>
    {
        private object? _value;

        public T Lazy(Func<T> fn)
        {
            if (fn == null) throw new ArgumentNullException(nameof(fn));
            // Ensure fn() result is boxed for LazyInitializer if T is a value type.
            Func<object?> objectFactory = () => fn();
            var result = LazyInitializer.EnsureInitialized(ref _value, objectFactory);
            return (T)result!;
        }

        private Maybe(T value)
        {
            _value = value;
        }

        public static Maybe<T> Some(T value)
        {
            return new Maybe<T>(value);
        }

        public static Maybe<T> None()
        {
            return new Maybe<T>();
        }

        public bool HasValue => _value != null;


        public T? ValueOrNull => (T?)_value;

        public T Value
        {
            get => _value != null ? (T)_value! : throw new InvalidOperationException("Maybe does not have a value");
            set => _value = value;
        }

        public T? ValueOrDefault(T? defaultValue = default)
        {
            return _value != null ? (T)_value! : defaultValue;
        }


        public Maybe<TResult> Select<TResult>(Func<T, TResult> map)
        {
            return _value != null ? Maybe<TResult>.Some(map((T)_value!)) : Maybe<TResult>.None();
        }

        public Maybe<TResult> SelectMany<TResult>(Func<T, Maybe<TResult>> bind)
        {
            return _value != null ? bind((T)_value!) : Maybe<TResult>.None();
        }

        public override string ToString()
        {
            return _value != null ? $"Some({_value})" : "None";
        }

        public static implicit operator Maybe<T>(T value)
        {
            return Some(value);
        }
    }
}