using System.Collections;
using MAVLinkAPI.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MAVLinkAPI.Tests.UI
{
    public class MutableComponentSpec
    {
        [UnityTest]
        public IEnumerator Replace_ReplacesComponentAndGameObject()
        {
            // Arrange
            var rootObject = new GameObject("Root");
            var originalObject = new GameObject("Original");
            originalObject.transform.SetParent(rootObject.transform);
            var originalComponent = originalObject.AddComponent<Text>();

            var templateObject = new GameObject("Template");
            templateObject.SetActive(false);
            var templateComponent = templateObject.AddComponent<Text>();

            var mutableComponent = new MutableComponent<Text> { mutable = originalComponent };

            // Act
            var newComponent = mutableComponent.CopyToReplace(templateComponent);
            yield return null; // Wait a frame for Destroy to take effect

            // Assert
            Assert.IsTrue(originalObject == null); // The old GameObject should be destroyed
            Assert.IsNotNull(newComponent);
            Assert.AreEqual(newComponent, mutableComponent.mutable);
            Assert.AreEqual("Template(Clone)", newComponent.name);
            Assert.AreEqual(rootObject.transform, newComponent.transform.parent);

            // Cleanup
            Object.Destroy(rootObject);
            Object.Destroy(templateObject);
        }
    }
}