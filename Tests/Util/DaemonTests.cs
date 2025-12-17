using NUnit.Framework;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using MAVLinkAPI.Util.Resource;
using UnityEngine;
using UnityEngine.TestTools;

namespace MAVLinkAPI.Tests.Util
{
    public class DaemonTests
    {
        private class TestDaemon : Daemon
        {
            public bool IsRunning { get; private set; }
            public int ExecuteCount { get; private set; }

            public TestDaemon(Lifetime lifetime) : base(lifetime)
            {
            }

            public override void Execute(CancellationToken cancelSignal)
            {
                ExecuteCount++;
                IsRunning = true;
                try
                {
                    while (!cancelSignal.IsCancellationRequested)
                        try
                        {
                            // Simulate work
                            Task.Delay(20, cancelSignal).Wait(cancelSignal);
                        }
                        catch (OperationCanceledException)
                        {
                            // Ignore cancellation, the loop condition will handle it
                        }
                }
                finally
                {
                    IsRunning = false;
                }
            }
        }

        [UnityTest]
        public IEnumerator Daemon_StartsAndStops()
        {
            using (var lifetime = new Lifetime())
            {
                var daemon = new TestDaemon(lifetime);

                Assert.IsFalse(daemon.IsRunning);
                Assert.AreEqual(0, daemon.ExecuteCount);

                daemon.Start();
                yield return new WaitForSeconds(0.05f);

                Assert.IsTrue(daemon.IsRunning);
                Assert.AreEqual(1, daemon.ExecuteCount);

                daemon.Stop();
                yield return new WaitForSeconds(0.05f);

                Assert.IsFalse(daemon.IsRunning);
            }
        }

        [Test]
        public void StopBlocking_WaitsForCompletion()
        {
            using (var lifetime = new Lifetime())
            {
                var daemon = new TestDaemon(lifetime);

                daemon.Start();
                Task.Delay(50).Wait();

                Assert.IsTrue(daemon.IsRunning);

                daemon.StopBlocking();

                Assert.IsFalse(daemon.IsRunning);
            }
        }

        [Test]
        public void Daemon_CannotBeRestarted()
        {
            // Arrange
            var lifetime = new Lifetime();
            var daemon = new TestDaemon(lifetime);

            // Act
            daemon.Start();
            daemon.StopBlocking();

            // Assert
            Assert.That(() => daemon.Start(), Throws.TypeOf<InvalidOperationException>());
            Assert.That(daemon.ExecuteCount, Is.EqualTo(1), $"Expected ExecuteCount: 1; Actual ExecuteCount: {daemon.ExecuteCount}");
        }

        [Test]
        public void Daemon_StartIsIdempotent()
        {
            using (var lifetime = new Lifetime())
            {
                var daemon = new TestDaemon(lifetime);

                Assert.AreEqual(0, daemon.ExecuteCount);

                daemon.Start();
                Thread.Sleep(50);
                Assert.IsTrue(daemon.IsRunning);
                Assert.AreEqual(1, daemon.ExecuteCount);

                // Call start again
                daemon.Start();
                Thread.Sleep(50);

                // ExecuteCount should still be 1
                Assert.AreEqual(1, daemon.ExecuteCount);
                Assert.IsTrue(daemon.IsRunning);

                daemon.StopBlocking();
            }
        }
    }
}