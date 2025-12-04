using System.Collections;
using MAVLinkAPI._Spike;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MAVLinkAPI.Tests.Spike
{
    public class DummyLayoutTest
    {
        private GameObject testObject;
        private RectTransform rectTransform;
        private DummyLayout dummyLayout;

        [SetUp]
        public void SetUp()
        {
            // Create a Canvas for proper UI layout calculations
            var canvasObject = new GameObject("TestCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            // Create test GameObject with RectTransform and add DummyLayout
            testObject = new GameObject("DummyLayoutTest");
            testObject.transform.SetParent(canvasObject.transform);

            rectTransform = testObject.AddComponent<RectTransform>();
            dummyLayout = testObject.AddComponent<DummyLayout>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null) Object.DestroyImmediate(testObject.transform.root.gameObject);
        }

        [UnityTest]
        public IEnumerator DummyLayout_ParentContentSizeFitterResizesToChildPreferredSize()
        {
            // Create a parent object with ContentSizeFitter
            var parentObject = new GameObject("ParentWithSizeFitter");
            parentObject.transform.SetParent(testObject.transform.parent);
            var parentRectTransform = parentObject.AddComponent<RectTransform>();

            // Move testObject to be child of parent
            testObject.transform.SetParent(parentObject.transform);

            // Set initial size to something different for both parent and child
            parentRectTransform.sizeDelta = new Vector2(100f, 100f);
            rectTransform.sizeDelta = new Vector2(100f, 100f);

            // Add ContentSizeFitter to parent to control sizing based on child's layout
            var sizeFitter = parentObject.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            parentObject.AddComponent<VerticalLayoutGroup>();

            // Wait a few frames for layout system to update
            yield return null;
            yield return null;

            // Force layout rebuild to ensure layout system processes the changes
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRectTransform);

            yield return null;

            // Check that parent RectTransform size has been updated to child's preferred size
            Assert.AreEqual(1000f, parentRectTransform.sizeDelta.x, 0.1f,
                "Parent RectTransform width should be set to child's preferred width (1000), but it is " +
                parentRectTransform.sizeDelta.x);
            Assert.AreEqual(1000f, parentRectTransform.sizeDelta.y, 0.1f,
                "Parent RectTransform height should be set to child's preferred height (1000), but it is " +
                parentRectTransform.sizeDelta.y);
        }
    }
}