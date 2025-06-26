using UnityEngine;
using UnityEngine.UI;

namespace MAVLinkAPI.Spike
{
    /// <summary>
    /// A dummy layout component that reports fixed preferred sizes.
    /// Can be used with ContentSizeFitter either on the same object (self-control)
    /// or on a parent object (parent control).
    /// </summary>
    public class DummyLayout : LayoutGroup, ILayoutSelfController
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            // Mark for layout rebuild when enabled to trigger parent ContentSizeFitter
            if (transform.parent != null)
            {
                var parentRect = transform.parent.GetComponent<RectTransform>();
                if (parentRect != null)
                {
                    LayoutRebuilder.MarkLayoutForRebuild(parentRect);
                }
            }
        }

        public override void CalculateLayoutInputHorizontal()
        {
            // Calculate and cache horizontal layout properties for parent ContentSizeFitter to read
            SetLayoutInputForAxis(minWidth, preferredWidth, flexibleWidth, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            // Calculate and cache vertical layout properties for parent ContentSizeFitter to read
            SetLayoutInputForAxis(minHeight, preferredHeight, flexibleHeight, 1);
        }

        public override void SetLayoutHorizontal()
        {
            // This controls how this layout positions its children horizontally
            // Since it's a dummy layout, we don't need to do anything
        }

        public override void SetLayoutVertical()
        {
            // This controls how this layout positions its children vertically  
            // Since it's a dummy layout, we don't need to do anything
        }

        // Report current size as preferred size
        public override float minWidth => 500;
        public override float minHeight => 500;
        public override float preferredHeight => 1000;
        public override float preferredWidth => 1000;
        public override float flexibleWidth => -1;
        public override float flexibleHeight => -1;
        public override int layoutPriority => 1;
    }
}