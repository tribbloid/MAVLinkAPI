using System.Collections;
using MAVLinkAPI.Scripts.Util.Resource;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MAVLinkAPI.Tests.Util.Resource
{
    public class LifetimeControllerIt
    {
        [UnityTest]
        public IEnumerator LifetimeController_GameObjectDestroyed_DisposesLifetime()
        {
            // Arrange
            var gameObject = new GameObject("TestGameObjectWithLifetimeController");
            var ctr = gameObject.AddComponent<LifetimeController>();

            var cl = new CleanableExample(ctr.ManagedLifetime);

            var v1 = CleanableExample.Counter;

            // Act
            Object.Destroy(gameObject);

            // Wait for the end of the frame for OnDestroy to be called
            yield return null;

            // Assert
            // The primary assertion is that the GameObject is destroyed.
            // NUnit's Is.Null assertion works correctly for destroyed Unity Objects.
            Assert.IsTrue(gameObject == null, "GameObject was not destroyed.");

            Assert.IsTrue(CleanableExample.Counter == v1 + 1);
            // Implicitly, if no errors occurred, Lifetime.Dispose was called successfully during OnDestroy.
        }
    }
}