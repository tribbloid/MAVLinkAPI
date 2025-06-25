using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace MAVLinkAPI.Tests.__Spike
{
    public class AsyncEnumerableSpike
    {
        private class MyAsyncEnumerable : IAsyncEnumerable<string>
        {
            public async IAsyncEnumerator<string> GetAsyncEnumerator(
                [EnumeratorCancellation] System.Threading.CancellationToken cancellationToken = default)
            {
                yield return "Hello";
                await Task.Delay(100, cancellationToken);
                yield return "Async";
                await Task.Delay(100, cancellationToken);
                yield return "World";
            }
        }

        [UnityTest]
        public IEnumerator TestMyAsyncEnumerable()
        {
            yield return AwaitTask(RunAsyncTest());
        }

        private async Task RunAsyncTest()
        {
            var asyncEnumerable = new MyAsyncEnumerable();
            var results = new List<string>();
            var expected = new List<string> { "Hello", "Async", "World" };

            await foreach (var item in asyncEnumerable) results.Add(item);

            CollectionAssert.AreEqual(expected, results);
        }

        private static IEnumerator AwaitTask(Task task)
        {
            while (!task.IsCompleted) yield return null;

            if (task.IsFaulted) throw task.Exception;
        }
    }
}