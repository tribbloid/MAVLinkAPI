#nullable enable
using System.Collections;
using Autofill;
using MAVLinkAPI.Util.Maybe;
using UI.Tables;
using UnityEngine;
using UnityEngine.UI;

namespace MAVLinkAPI.UI
{
    public class FoldingPanelBehaviour : MonoBehaviour
    {
        
        [Autofill(AutofillType.Parent)] public TableLayout table;

        [Autofill(AutofillType.Parent)] public TableRow row;

        [Required] public Button toggle;
        public MonoBehaviour? detail;

        public void Start()
        {
            StartCoroutine(UpdateHeightsAfterLayout());
            toggle.onClick.AddListener(() =>
            {
                detail?.gameObject.SetActive(!detail.gameObject.activeSelf);
                StartCoroutine(UpdateHeightsAfterLayout());
            });
        }


        private IEnumerator UpdateHeightsAfterLayout()
        {
            // Wait until the end of the frame, after layout calculations
            yield return new WaitForEndOfFrame();

            // Now get the updated height
            row.preferredHeight = GetComponent<RectTransform>()!.rect.height + 10;

            // row.UpdateLayout(); // Update the layout.CellCount
            table.UpdateLayout();
        }
    }

    // public class TableController : MonoBehaviour
    // {
    //     [Autofill] public TableLayout table;
    //
    //     [Required] public TableRow rowTemplate;
    // }
}