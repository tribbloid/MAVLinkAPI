#nullable enable
using System;
using System.Collections;
using Autofill;
using MAVLinkAPI.UI.Tables;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;
using UnityEngine.UI;

namespace MAVLinkAPI.UI.TableExt
{
    [RequireComponent(typeof(RectTransform))]
    public class FoldableCell : MonoBehaviour
    {
        [Autofill] public RectTransform rectT = null!;

        [Autofill(AutofillType.SelfAndParent)] public TableRow row = null!;
        [Autofill(AutofillType.Parent)] public ScrollLock scrollLock = null!;

        // [Required] public ScrollLock scrollLock = null!;

        // [Autofill(AutofillType.Parent)] public TableLayout table = null!;

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
            StartCoroutine(UpdateHeightsTask(wait));
        }

        private IEnumerator UpdateHeightsTask(bool wait)
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

            foreach (var o in scrollLock.RefreshTable())
            {
                yield return o; // TODO: how to simplify this?
            }

            // row.UpdateLayout(); // doesn't work
            // table.UpdateLayout(); // reset the scroll position
            // LayoutRebuilder.MarkLayoutForRebuild(table.GetComponent<RectTransform>()); // ditto

            // var tempRow = table.AddRow();
            // Destroy(tempRow.gameObject); // ditto
        }
    }
}