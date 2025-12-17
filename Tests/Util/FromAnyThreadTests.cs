using NUnit.Framework;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using MAVLinkAPI.Util;
using UnityEngine;
using UnityEngine.TestTools;

namespace MAVLinkAPI.Tests.Util
{
    public class FromAnyThreadTests
    {
        [SetUp]
        public void SetUp()
        {
            // Ensure dispatcher exists on main thread for tests that create worker threads.
            FromAnyThread.Initialize();
        }

        [Test]
        public void Queue_OnMainThread_CompletesImmediately()
        {
            var task = FromAnyThread.Queue(() => 42);
            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsFaulted);
            Assert.AreEqual(42, task.Result);
        }

        [Test]
        public void QueueAction_OnMainThread_CompletesImmediately()
        {
            var called = false;
            var task = FromAnyThread.Queue(() => { called = true; });
            Assert.IsTrue(called);
            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsFaulted);
        }

        [UnityTest]
        public IEnumerator QueueAction_FromWorkerThread_CompletesAfterMainThreadUpdate()
        {
            var called = false;

            Task task = null;
            var worker = new Thread(() => { task = FromAnyThread.Queue(() => { called = true; }); });

            worker.Start();
            worker.Join();

            Assert.IsNotNull(task);
            Assert.IsFalse(task.IsCompleted);
            Assert.IsFalse(called);

            yield return null;

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsFaulted);
            Assert.IsTrue(called);
        }

        [UnityTest]
        public IEnumerator Queue_FromWorkerThread_CompletesAfterMainThreadUpdate()
        {
            Task<int> task = null;
            var worker = new Thread(() => { task = FromAnyThread.Queue(() => 123); });

            worker.Start();
            worker.Join();

            Assert.IsNotNull(task);
            Assert.IsFalse(task.IsCompleted);

            yield return null;

            Assert.IsTrue(task.IsCompleted);
            Assert.IsFalse(task.IsFaulted);
            Assert.AreEqual(123, task.Result);
        }

        [UnityTest]
        public IEnumerator Queue_FromWorkerThread_PropagatesException()
        {
            Task<int> task = null;
            var worker = new Thread(() =>
            {
                task = FromAnyThread.Queue<int>(() => throw new InvalidOperationException("boom"));
            });

            worker.Start();
            worker.Join();

            Assert.IsNotNull(task);
            Assert.IsFalse(task.IsCompleted);

            yield return null;

            Assert.IsTrue(task.IsCompleted);
            Assert.IsTrue(task.IsFaulted);
        }

        [UnityTest]
        public IEnumerator Instantiate_OnMainThread_CompletesImmediately()
        {
            var prefab = new GameObject("FromAnyThreadTests_Prefab");

            Task<GameObject> task;
            try
            {
                task = FromAnyThread.Instantiate(prefab, Vector3.zero, Quaternion.identity, null);
                Assert.IsTrue(task.IsCompleted);
                Assert.IsFalse(task.IsFaulted);

                var instance = task.Result;
                Assert.IsNotNull(instance);

                UnityEngine.Object.DestroyImmediate(instance);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefab);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Instantiate_FromWorkerThread_CompletesAfterMainThreadUpdate()
        {
            var prefab = new GameObject("FromAnyThreadTests_Prefab");

            Task<GameObject> task = null;
            var worker = new Thread(() =>
            {
                task = FromAnyThread.Instantiate(prefab, Vector3.zero, Quaternion.identity, null);
            });

            try
            {
                worker.Start();
                worker.Join();

                Assert.IsNotNull(task);
                Assert.IsFalse(task.IsCompleted);

                // Let dispatcher Update() run.
                yield return null;

                Assert.IsTrue(task.IsCompleted);
                Assert.IsFalse(task.IsFaulted);

                var instance = task.Result;
                Assert.IsNotNull(instance);

                UnityEngine.Object.DestroyImmediate(instance);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(prefab);
            }
        }

        [UnityTest]
        public IEnumerator Destroy_FromWorkerThread_CompletesAndObjectIsDestroyed()
        {
            var go = new GameObject("FromAnyThreadTests_ToDestroy");

            Task destroyTask = null;
            var worker = new Thread(() => { destroyTask = FromAnyThread.Destroy(go); });

            worker.Start();
            worker.Join();

            Assert.IsNotNull(destroyTask);
            Assert.IsFalse(destroyTask.IsCompleted);

            yield return null;

            Assert.IsTrue(destroyTask.IsCompleted);
            Assert.IsFalse(destroyTask.IsFaulted);

            // Unity overloads == for destroyed objects.
            Assert.IsTrue(go == null);
        }
    }
}