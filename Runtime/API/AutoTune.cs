#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MAVLinkAPI.Routing;
using MAVLinkAPI.Util;
using UnityEngine;

namespace MAVLinkAPI.API
{
    public sealed class AutoTune
    {
        // used to optimise baud rate and other variables based on execution outcome

        private readonly IReadOnlyList<int> _preferredBaudRates;
        private readonly TimeSpan _timeout;
        private readonly bool _disconnectFirst;

        public AutoTune(
            IReadOnlyList<int>? preferredBaudRates = null,
            TimeSpan? timeout = null,
            bool disconnectFirst = true)
        {
            _preferredBaudRates = preferredBaudRates ?? IOStream.BaudRates.preferred;
            _timeout = timeout ?? TimeSpan.FromSeconds(10);
            _disconnectFirst = disconnectFirst;
        }

        private CancellationTokenSource _cts = new();

        public T OnStream<T>(
            IOStream io,
            Func<IOStream, T> func
        )
        {
            var token = _cts.Token;

            if (_preferredBaudRates.Count == 0) return Run(token);

            var result = _preferredBaudRates.Retry().With(
                    TimeSpan.FromSeconds(0.2),
                    (i, j) => token.IsCancellationRequested
                )
                .FixedInterval.Run((baudRate, i) =>
                    {
                        io.BaudRate = baudRate;
                        return Run(token);
                    }
                );

            return result;

            T Run(CancellationToken _)
            {
                try
                {
                    var taskCompletedSuccessfully = false;

                    if (_disconnectFirst) io.Disconnect();
                    io.Connect();

                    var task = Task.Run(() =>
                    {
                        try
                        {
                            var rr = func(io);
                            taskCompletedSuccessfully = true;
                            return rr;
                        }
                        finally
                        {
                            if (!taskCompletedSuccessfully)
                            {
                                Debug.LogWarning("task terminated, cleaning up");
                                io.Disconnect();
                            }
                        }
                    });

                    if (task.Wait(_timeout))
                    {
                        Debug.Log("Handshake completed");
                        return task.Result;
                    }

                    throw new TimeoutException($"Timeout after {_timeout.TotalSeconds} seconds");
                }
                catch
                {
                    io.Disconnect();
                    throw;
                    // errors[baud] = new Exception("Failed to connect");
                }
            }
        }
    }
}