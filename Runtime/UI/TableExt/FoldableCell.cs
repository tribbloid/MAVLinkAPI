#nullable enable
using System;
using System.Collections;
using Autofill;
using MAVLinkAPI.UI.Tables;
using MAVLinkAPI.Util.NullSafety;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MAVLinkAPI.UI.TableExt
{
    [RequireComponent(typeof(RectTransform))]
    public class FoldableCell : UIBehaviour
    {
        [Autofill] public RectTransform rectT = null!;

        [Autofill(AutofillType.SelfAndParent)] public TableRow row = null!;

        [Autofill(AutofillType.Parent)] public TableLayout table = null!;

        [Required] public Button toggle = null!;
        public MonoBehaviour? detail;

        private float _minHeight = -1;

        protected override void Start()
        {
            _minHeight = row.preferredHeight;

            // detail?.gameObject.SetActive(false); // enable to this to fold panel on start
            UpdateHeights(true);

            toggle.onClick.AddListener(() =>
            {
                detail?.gameObject.SetActive(!detail.gameObject.activeSelf);
                // UpdateHeights();
            });
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            // Debug.Log("RectTransform size changed!");
            UpdateHeights();
            // Place your custom callback logic here
        }

        protected override void OnEnable()
        {
            UpdateHeights(true);
        }

        private void UpdateHeights(bool wait = false)
        {
            if (isActiveAndEnabled) StartCoroutine(UpdateHeightsTask(wait));
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

            table.UpdateLayout();
        }
    }
}