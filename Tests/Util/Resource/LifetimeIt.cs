using System.Collections;
using MAVLinkAPI.Util.Resource;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MAVLinkAPI.Tests.Util.Resource
{
    public class LifetimeIt
    {
        [UnityTest]
        public IEnumerator LifetimeController_GameObjectDestroyed_DisposesLifetime()
        {
            // Arrange
            var gameObject = new GameObject("TestGameObjectWithLifetimeController");
            var ctr = gameObject.AddComponent<LifetimeBinding>();

            var cl = new CleanableExample(ctr.Lifetime);

            var v1 = CleanableExample.Counter.Value;

            // Act
            Object.Destroy(gameObject);

            // Wait for the end of the frame for OnDestroy to be called
            yield return null;

            // Assert
            // The primary assertion is that the GameObject is destroyed.
            // NUnit's Is.Null assertion works correctly for destroyed Unity Objects.
            Assert.IsTrue(gameObject == null, "GameObject was not destroyed.");

            Assert.AreEqual(CleanableExample.Counter.Value, v1 + 1, "lifetime callback is not triggered");

            Assert.IsTrue(ctr.Lifetime.IsClosed, "lifetime is not clean");
            // Implicitly, if no errors occurred, Lifetime.Dispose was called successfully during OnDestroy.
        }
    }
}