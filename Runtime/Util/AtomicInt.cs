using System.Threading;

namespace MAVLinkAPI.Util
{
    public class Atomic<T>
    {
        protected T ValueInternal;
        public long UpdateCount = 0; // counter is not synchronised

        public Atomic(T initialValue = default)
        {
            ValueInternal = initialValue;
        }

        public T Value
        {
            get
            {
                lock (this)
                {
                    return ValueInternal;
                }
            }
            set
            {
                lock (this)
                {
                    ValueInternal = value;
                }

                Interlocked.Increment(ref UpdateCount);
            }
        }

        public override string ToString() => Value.ToString();
    }

    public class AtomicInt : Atomic<int>
    {
        public int Increment()
        {
            var result = Interlocked.Increment(ref ValueInternal);
            Interlocked.Increment(ref UpdateCount);
            return result;
        }

        public int Decrement()
        {
            var result = Interlocked.Decrement(ref ValueInternal);
            Interlocked.Increment(ref UpdateCount);
            return result;
        }

        public int Add(int value)
        {
            var result = Interlocked.Add(ref ValueInternal, value);
            Interlocked.Increment(ref UpdateCount);
            return result;
        }
    }

    public class AtomicLong : Atomic<long>
    {
        public long Increment()
        {
            var result = Interlocked.Increment(ref ValueInternal);
            Interlocked.Increment(ref UpdateCount);
            return result;
        }

        public long Decrement()
        {
            var result = Interlocked.Decrement(ref ValueInternal);
            Interlocked.Increment(ref UpdateCount);
            return result;
        }

        public long Add(long value)
        {
            var result = Interlocked.Add(ref ValueInternal, value);
            Interlocked.Increment(ref UpdateCount);
            return result;
        }
    }
}