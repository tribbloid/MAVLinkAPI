#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;

namespace MAVLinkAPI.Util.Resource
{
    public abstract class Daemon : Cleanable
    {
        // Can only be started once and canceled once
        public readonly CancellationTokenSource Cancel = new();


        public Daemon(Lifetime lifetime) : base(lifetime)
        {
        }

        public readonly int GraceTimeMillis = 5000;

        private Maybe<Task> _started;

        public void Start()
        {
            if (Cancel.IsCancellationRequested)
                throw new InvalidOperationException("Daemon has been stopped and cannot be restarted.");

            _started.Lazy(() =>
            {
                var cancelSignal = Cancel.Token;

                return Task.Run(() =>
                    {
                        try
                        {
                            Execute(cancelSignal);
                        }
                        catch (Exception e)
                        {
                            if (e is not OperationCanceledException) Debug.LogException(e);
                        }
                    }, cancelSignal
                );
            });
        }


        public void Stop()
        {
            if (!Cancel.IsCancellationRequested) Cancel.Cancel();
        }

        public void StopBlocking()
        {
            Stop();
            try
            {
                _started.ValueOrNull?.Wait(GraceTimeMillis);
            }
            catch (AggregateException e)
            {
                e.Handle(ex => ex is OperationCanceledException);
            }
            catch (OperationCanceledException)
            {
                // also possible
            }
        }

        public override void DoClean()
        {
            StopBlocking();
            Cancel.Dispose();
        }

        public abstract void Execute(CancellationToken cancelSignal);
    }

    // once created, will repeatedly do something
    public abstract class RecurrentDaemon : Daemon
    {
        public readonly AtomicLong Counter = new();

        protected RecurrentDaemon(Lifetime lifetime) : base(lifetime)
        {
        }

        public override void Execute(CancellationToken cancelSignal)
        {
            while (!cancelSignal.IsCancellationRequested) // soft cancel immediately
            {
                // TODO: need to control frequency
                Counter.Increment();
                Iterate();
            }
        }

        protected abstract void Iterate();
    }
}