#nullable enable
using System;
using System.Collections;
using Autofill;
using MAVLinkAPI.Util.NullSafety;
using UI.Tables;
using UnityEngine;
using UnityEngine.UI;

namespace MAVLinkAPI.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class FoldingPanelBehaviour : MonoBehaviour
    {
        [Autofill] public RectTransform rectT;
        
        [Autofill(AutofillType.Parent)] public TableLayout table;

        [Autofill(AutofillType.Parent)] public TableRow row;

        [Required] public Button toggle;
        public MonoBehaviour? detail;

        private float minHeight = -1;

        public void Start()
        {
            minHeight = row.preferredHeight;

            StartCoroutine(UpdateHeightsAfterLayout());
            toggle.onClick.AddListener(() =>
            {
                detail?.gameObject.SetActive(!detail.gameObject.activeSelf);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectT);
                StartCoroutine(UpdateHeightsAfterLayout());
            });
        }


        private IEnumerator UpdateHeightsAfterLayout()
        {
            // Wait until the end of the frame, after layout calculations
            yield return new WaitForEndOfFrame();

            // Now get the updated height
            row.preferredHeight = Math.Max(
                GetComponent<RectTransform>()!.rect.height + 10,
                minHeight
            );

            // row.UpdateLayout(); // Update the layout.CellCount
            table.UpdateLayout();
        }
    }
}