using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MAVLinkAPI.Util
{
    public static class FromAnyThread
    {
        // the following functions are safe to be used in any thread, not just Unity main thread
        // if calling from the main thread, will execute immediately
        // if calling from any other thread, will execute on the main thread and return a task that will complete when the operation is finished

        private static int? _mainThreadId;
        private static MainThreadDispatcher _dispatcher;
        private static readonly ConcurrentQueue<Action> _queue = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            // Ensure initialization happens on main thread early in play-mode.
            Initialize();
        }

        public static void Initialize()
        {
            if (_dispatcher != null) return;

            _mainThreadId = Thread.CurrentThread.ManagedThreadId;

            var go = new GameObject("MAVLinkAPI.FromAnyThread");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _dispatcher = go.AddComponent<MainThreadDispatcher>();
        }

        private static bool IsMainThread()
        {
            // If we haven't been initialized yet, assume current thread is main thread.
            return _mainThreadId == null || _mainThreadId == Thread.CurrentThread.ManagedThreadId;
        }

        private static void Enqueue(Action action)
        {
            if (_dispatcher == null)
            {
                throw new InvalidOperationException(
                    "FromAnyThread is not initialized. Call MAVLinkAPI.Util.FromAnyThread.Initialize() on Unity main thread before using it from background threads.");
            }

            _queue.Enqueue(action);
        }

        public static Task<T> Instantiate<T>(
            T original,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
            where T : UnityEngine.Object
        {
            if (IsMainThread())
            {
                if (_dispatcher == null) Initialize();
                return Task.FromResult(UnityEngine.Object.Instantiate(original, position, rotation, parent));
            }

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            Enqueue(() =>
            {
                try
                {
                    var instance = UnityEngine.Object.Instantiate(original, position, rotation, parent);
                    tcs.TrySetResult(instance);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            });

            return tcs.Task;
        }

        public static Task Destroy(UnityEngine.Object obj)
        {
            if (IsMainThread())
            {
                if (_dispatcher == null) Initialize();
                UnityEngine.Object.Destroy(obj);
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            Enqueue(() =>
            {
                try
                {
                    UnityEngine.Object.Destroy(obj);
                    tcs.TrySetResult(null);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            });

            return tcs.Task;
        }

        private sealed class MainThreadDispatcher : MonoBehaviour
        {
            private void Update()
            {
                while (_queue.TryDequeue(out var action)) action();
            }
        }
    }
}