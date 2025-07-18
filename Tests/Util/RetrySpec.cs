using System;
using System.Collections.Generic;
using System.Threading;
using MAVLinkAPI.Util;
using NUnit.Framework;

namespace MAVLinkAPI.Tests.Util
{
    [TestFixture]
    public class RetrySpec
    {
        [Test]
        public void Retry_SucceedsOnFirstAttempt()
        {
            var items = new List<int> { 1, 2, 3 };
            var result = items.Retry().FixedInterval.Run((i, elapsed) => i == 1 ? i : throw new Exception("Failed"));

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void Retry_SucceedsOnSecondItem()
        {
            var items = new List<string> { "a", "b", "c" };
            var result = items.Retry().FixedInterval.Run((s, elapsed) => s == "b" ? s : throw new Exception("Failed"));

            Assert.That(result, Is.EqualTo("b"));
        }

        [Test]
        public void Retry_FailsAllAttempts()
        {
            var items = new List<int> { 1, 2, 3 };

            Assert.Throws<RetryException>(() =>
                items.Retry().FixedInterval.Run((i, elapsed) => throw new Exception("Always fails"))
            );
        }

        [Test]
        public void Retry_RespectsMaxAttempts()
        {
            var items = new List<int> { 1, 2 };
            var attemptCount = 0;

            Assert.Throws<RetryException>(() =>
                items.Retry().FixedInterval.Run((i, elapsed) =>
                {
                    attemptCount++;
                    throw new Exception("Failed");
                })
            );

            Assert.That(attemptCount, Is.EqualTo(2));
        }

        [Test]
        public void Retry_ReturnsImmediatelyOnSuccess()
        {
            var items = new List<int> { 1, 2, 3 };
            var processedCount = 0;

            var result = items.Retry().FixedInterval.Run((i, elapsed) =>
            {
                processedCount++;
                return i == 2 ? i * 10 : throw new Exception("Failed");
            });

            Assert.That(result, Is.EqualTo(20));
            Assert.That(processedCount, Is.EqualTo(2));
        }

        [Test]
        public void Retry_ProvidesCorrectElapsedTime()
        {
            var items = new List<int> { 1, 2 };
            var capturedElapsed = new List<TimeSpan>();

            items.Retry().With(TimeSpan.FromMilliseconds(100))
                .FixedInterval.Run((i, elapsed) =>
                {
                    capturedElapsed.Add(elapsed);
                    Thread.Sleep(100); // Simulate some work

                    if (i <= 1)
                        throw new Exception("First attempt fails");
                });

            Assert.That(capturedElapsed, Is.Not.Empty);
            Assert.That(capturedElapsed[1].TotalMilliseconds, Is.GreaterThanOrEqualTo(100));
        }

        [Test]
        public void RetryException_ToString()
        {
            var items = new List<int> { 1, 1, 2, 2 };
            RetryException exception = null;

            try
            {
                items.Retry().FixedInterval.Run((i, elapsed) => throw new Exception($"Failure on item {i}"));
            }
            catch (RetryException e)
            {
                exception = e;
            }

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Does.StartWith("All 4 attempt(s) failed:"));
            Assert.That(exception.Message, Does.Contain("- (x2) Failure on item 1"));
            Assert.That(exception.Message, Does.Contain("- (x2) Failure on item 2"));
            Assert.That(exception.Source, Is.EqualTo("ca.hpvdt.mavlinkapi.Runtime"));
        }
    }
}