#nullable enable
using System;
using System.Collections;
using Autofill;
using MAVLinkAPI.UI.Tables;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;
using UnityEngine.UI;

namespace MAVLinkAPI.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class FoldableCellPanel : MonoBehaviour
    {
        [Autofill] public RectTransform rectT = null!;

        [Autofill(AutofillType.Parent)] public TableLayout table = null!;

        [Autofill(AutofillType.Parent)] public TableRow row = null!;

        [Required] public Button toggle = null!;
        public MonoBehaviour? detail;

        private float _minHeight = -1;

        public void Start()
        {
            _minHeight = row.preferredHeight;

            UpdateHeights(true);

            toggle.onClick.AddListener(() =>
            {
                detail?.gameObject.SetActive(!detail.gameObject.activeSelf);
                UpdateHeights();
            });
        }

        private void UpdateHeights(bool wait = false)
        {
            StartCoroutine(UpdateHeightsAsync(wait));
        }

        private IEnumerator UpdateHeightsAsync(bool wait)
        {
            // Wait until the end of the frame, after layout calculations
            if (wait) yield return new WaitForEndOfFrame();

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectT);
            // rectT.ForceUpdateRectTransforms();

            yield return new WaitForEndOfFrame();

            // Now get the updated height
            row.preferredHeight = Math.Max(
                rectT.rect.height + 10,
                _minHeight
            );

            // row.UpdateLayout(); // Update the layout.CellCount
            table.UpdateLayout();
        }
    }
}