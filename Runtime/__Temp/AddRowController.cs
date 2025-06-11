#nullable enable
using MAVLinkAPI.UI.Tables;
using UnityEngine;
using UnityEngine.UI;

public class AddRowController : MonoBehaviour
{
    [SerializeField] private TableLayout tableLayout;

    [SerializeField] private RectTransform? outerContent;

    [SerializeField] private string[] placeholderTexts = new string[] { "Item", "Description", "Value" };


    public void AddRow()
    {
        if (tableLayout != null)
        {
            // Create a new row instance first
            var rowGameObject = TableLayoutUtilities.InstantiatePrefab("UI/Tables/Row");
            rowGameObject.name = "Row";

            // Get the TableRow component
            var newRow = rowGameObject.GetComponent<TableRow>();
            // newRow.preferredHeight = 50f;

            // Add cells with placeholder text
            for (int i = 0; i < placeholderTexts.Length; i++)
            {
                // var cell = new TableCell();
                // Create a cell
                var cell = newRow.AddCell();

                // Create text GameObject
                GameObject textObject = new GameObject("Text", typeof(RectTransform));
                textObject.transform.SetParent(cell.transform);

                // Add and configure Text component
                Text tt = textObject.AddComponent<Text>();
                tt.text = placeholderTexts[i];
            }

            // Finally, add the configured row to the tableLayout
            tableLayout.AddRow(newRow);
        }
    }
}