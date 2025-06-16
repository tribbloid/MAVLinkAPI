using System;
using System.Threading;
using System.Threading.Tasks;
using MAVLinkAPI.Util.Resource;
using UnityEngine;

namespace MAVLinkAPI.Util
{
    public abstract class Daemon : Cleanable
    {
        // Can only be canceled once, as a result, it can only be started once
        public CancellationTokenSource Cancel = new();

        private CancellationToken? _cancelSignal;

        public Daemon(Lifetime lifetime) : base(lifetime)
        {
        }

        // ~Daemon()
        // {
        //     Dispose();
        // }

        public readonly int GraceTimeMillis = 5000;

        public void Stop()
        {
            Cancel.CancelAfter(GraceTimeMillis); // Execute() should drop out of the loop first
            _cancelSignal = null;
        }

        protected override void DoClean()
        {
            Stop();
        }

        public async void Start()
        {
            var cancelSignal = Cancel.Token;
            _cancelSignal = cancelSignal;
            await Task.Run(() =>
                {
                    try
                    {
                        Execute(cancelSignal);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }, cancelSignal // hard cancel after 5 seconds
            );
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