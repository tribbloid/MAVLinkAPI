#nullable enable
using System.Collections;
using MAVLinkAPI.Util.Maybe;
using TMPro;
using UI.Tables;
using UnityEngine;
using UnityEngine.UI;
using Autofill;
using MAVLinkAPI.UI;

namespace MAVLinkAPI.Util.Resource
{
    public class LifetimeTableController : LifetimeInScene
    {
        [Required] public TableLayout table;

        // public TableController primary;

        public class ToggleDetailBehaviour : MonoBehaviour
        {
            [Required] public TableRow row;

            [Required] public Button summary;
            [Required] public TextMeshProUGUI? detail;

            public void Start()
            {
                summary.onClick.AddListener(() => detail?.gameObject.SetActive(!detail.gameObject.activeSelf));
            }
        }

        public class OfRow : MonoBehaviour
        {
            [Required] public LifetimeTableController outer;

            public TableLayout table => outer.table;

            [Required] public TableRow row;

            [Required] public Image Icon;

            [Required] public TextMeshProUGUI summary;
            public TextMeshProUGUI? detail;

            // if both are ticked the instance will be terminated
            [Required] public Toggle terminate1;
            [Required] public Toggle terminate2;

            public void Start()
            {
            }
        }
    }
}