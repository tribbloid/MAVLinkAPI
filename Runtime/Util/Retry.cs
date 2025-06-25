#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MAVLinkAPI.Ext;
using MAVLinkAPI.Util.Text;
using Debug = UnityEngine.Debug;

namespace MAVLinkAPI.Util
{
    public class Retry<TI>
    {
        public List<TI> Attempts = null!;

        private ArgsT? _args;

        public ArgsT Args => _args ?? DefaultArgs;

        public string Name = "Retry-" + Retry.NameCounter.Increment();

        private static bool DefaultShouldContinue(Exception ex, TI attempt)
        {
            return true;
        }

        private static readonly ArgsT DefaultArgs = new(
            TimeSpan.FromSeconds(1),
            DefaultShouldContinue
        );

        public Retry<TI> With(
            TimeSpan? interval = null,
            Func<Exception, TI, bool>? shouldContinue = null,
            bool logException = false,
            string? name = null
        )

        {
            _args = new ArgsT(
                interval ?? DefaultArgs.Interval,
                shouldContinue ?? DefaultArgs.ShouldContinue,
                logException
            );

            if (name != null) Name = name;
            return this;
        }

        public record ArgsT(
            TimeSpan Interval,
            Func<Exception, TI, bool> ShouldContinue,
            bool LogException = false
        );

        public class FixedIntervalT : HasOuter<Retry<TI>>
        {
            public FixedIntervalT(Retry<TI> outer)
            {
                Outer = outer;
            }

            public T Run<T>(Func<TI, TimeSpan, T> operation)
            {
                if (operation == null)
                    throw new ArgumentNullException(nameof(operation));

                var errors = new List<(TI, Exception)>();

                var stopwatch = Stopwatch.StartNew();

                var counter = 0;

                var zipped = Outer.Attempts.ZipWithNext().ToList();

                foreach (var (attempt, next) in zipped)
                {
                    T result = default!;
                    try
                    {
                        result = operation(attempt, stopwatch.Elapsed);

                        return result;
                    }
                    catch (Exception ex)
                    {
                        if (Outer.Args.LogException) Debug.LogException(ex);
                        errors.Add((attempt, ex));

                        var baseInfo =
                            $"{Outer.Name}/[{counter}/{zipped.Count}] {attempt}: Retry failed @ {stopwatch.Elapsed}s:" +
                            $"\n{ex.Message}";

                        // TODO: should stop retry if the current thread is cancelled with a token
                        if (!Outer.Args.ShouldContinue(ex, attempt) || next == null)
                        {
                            // Debug.Log(baseInfo + $"\nthis is the last");

                            var info = $"All {counter + 1} attempt(s) failed";

                            var ee = new RetryException(
                                info,
                                errors.Select(kv => kv.Item2)
                            );

                            throw ee;
                        }

                        Debug.Log(baseInfo + $"\nwill try again at [{next.Value}]");

                        Thread.Sleep(Outer.Args.Interval);
                    }

                    counter += 1;
                }

                throw new SystemException("IMPOSSIBLE!");
            }

            public void Run(Action<TI, TimeSpan> operation)
            {
                if (operation == null)
                    throw new ArgumentNullException(nameof(operation));

                Run<object>((attempt, elapsed) =>
                {
                    operation(attempt, elapsed);
                    return null!;
                });
            }
        }

        public FixedIntervalT FixedInterval => new(this);
    }

    public static class Retry
    {
        public static AtomicLong NameCounter = new();

        public static Retry<int> UpTo(int maxAttempts)
        {
            return new Retry<int>
            {
                Attempts = Enumerable.Range(0, maxAttempts).ToList()
            };
        }
    }

    public class RetryException : Exception
    {
        // constructors
        public RetryException(string message) : base(message)
        {
        }


        private static string CompileMessage(string message, IEnumerable<Exception> causes)
        {
            var causeSummary = causes
                .GroupBy(e => e.Message)
                .Select(g =>
                    new TextBlock("-")
                        .ZipRight(
                            new TextBlock($" (x{g.Count()}) ")
                        )
                        .ZipRight(
                            new TextBlock(g.Key)
                        )
                        .PadLeft()
                );

            return $"{message}:\n{string.Join("\n", causeSummary)}\n";
        }

        public RetryException(string message, IEnumerable<Exception> causes) : base(
            CompileMessage(message, causes),
            causes.FirstOrDefault()
        )
        {
        }
    }

    public static class RetryExtensions
    {
        public static Retry<TI> Retry<TI>(
            this IEnumerable<TI> attempts,
            string? name = null
        )
        {
            var result = new Retry<TI>
            {
                Attempts = attempts.ToList()
            };

            if (name != null) result.Name = name;

            return result;
        }
    }
}