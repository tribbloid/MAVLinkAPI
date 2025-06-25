using NUnit.Framework;
using UnityEngine;
using MAVLinkAPI.Ext;

namespace MAVLinkAPI.Tests.Ext
{
    public class GameObjectExtensionsSpec
    {
        private GameObject _root, _child, _grandchild;

        [SetUp]
        public void SetUp()
        {
            // Create a hierarchy of GameObjects for testing
            _root = new GameObject("Root");
            _child = new GameObject("Child");
            _grandchild = new GameObject("Grandchild");

            _child.transform.SetParent(_root.transform);
            _grandchild.transform.SetParent(_child.transform);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the created GameObjects
            Object.DestroyImmediate(_root);
            // Child and grandchild are destroyed as children of root
        }

        [Test]
        public void GetScenePath_ForRootObject()
        {
            // Act
            string path = _root.GetScenePath();

            // Assert
            Assert.AreEqual("/Root", path);
        }

        [Test]
        public void GetScenePath_ForNestedObject()
        {
            // Act
            string path = _child.GetScenePath();

            // Assert
            Assert.AreEqual("/Root/Child", path);
        }

        [Test]
        public void GetScenePath_ForDeeplyNestedObject()
        {
            // Act
            string path = _grandchild.GetScenePath();

            // Assert
            Assert.AreEqual("/Root/Child/Grandchild", path);
        }

        [Test]
        public void GetScenePath_ForNullObject()
        {
            // Arrange
            GameObject nullObject = null;

            // Act
            string path = nullObject.GetScenePath();

            // Assert
            Assert.AreEqual("", path);
        }
    }
}