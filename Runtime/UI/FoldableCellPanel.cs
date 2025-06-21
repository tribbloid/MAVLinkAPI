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

            UpdateHeights();

            toggle.onClick.AddListener(() =>
            {
                detail?.gameObject.SetActive(!detail.gameObject.activeSelf);
                UpdateHeights();
            });
        }

        private void UpdateHeights()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectT);
            StartCoroutine(UpdateHeightsAfterLayout());
        }

        private IEnumerator UpdateHeightsAfterLayout()
        {
            // Wait until the end of the frame, after layout calculations
            yield return new WaitForEndOfFrame();

            // Now get the updated height
            row.preferredHeight = Math.Max(
                GetComponent<RectTransform>()!.rect.height + 10,
                _minHeight
            );

            // row.UpdateLayout(); // Update the layout.CellCount
            table.UpdateLayout();
        }
    }
}