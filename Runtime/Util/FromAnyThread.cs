using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MAVLinkAPI.Util
{
    public static class FromAnyThread
    {
        private readonly struct Unit
        {
            public static readonly Unit Value = new();
        }

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

        // the following functions are safe to be used in any thread, not just Unity main thread
        // if calling from the main thread, will execute immediately
        // if calling from any other thread, will execute on the main thread and return a task that will complete when the operation is finished
        public static Task<T> Queue<T>(Func<T> fn)
        {
            if (fn == null) throw new ArgumentNullException(nameof(fn));

            if (IsMainThread())
            {
                return Task.FromResult(fn());
            }

            var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            Enqueue(() =>
            {
                try
                {
                    tcs.TrySetResult(fn());
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            });

            return tcs.Task;
        }

        public static Task Queue(Action fn)
        {
            if (fn == null) throw new ArgumentNullException(nameof(fn));

            return Queue(() =>
            {
                fn();
                return Unit.Value;
            });
        }

        public static Task<T> Instantiate<T>(
            T original,
            Vector3 position,
            Quaternion rotation,
            Transform parent)
            where T : UnityEngine.Object
        {
            return Queue(() => UnityEngine.Object.Instantiate(original, position, rotation, parent));
        }

        public static Task Destroy(UnityEngine.Object obj)
        {
            return Queue(() => UnityEngine.Object.Destroy(obj));
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