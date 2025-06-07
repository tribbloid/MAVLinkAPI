#nullable enable
using Autofill;
using MAVLinkAPI.Util.NullSafety;
using TMPro;
using UI.Tables;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MAVLinkAPI.Util.Resource
{
    public class CleanableRowBinding : MonoBehaviour
    {
        // public class Schema
        //
        // [Required] public Cleanable Cleanable;
        [Autofill] public TableRow row;

        [Autofill] public Image icon;

        [Required] public TextMeshProUGUI summary;
        [Required] public TextMeshProUGUI detail;

        // if both are ticked the instance will be terminated
        [Required] public Toggle terminate1;
        [Required] public Toggle terminate2;

        [Serialize] private float updateFreqSec = 1f;

        [DoNotSerialize] public Cleanable? value;

        private void Start()
        {
            terminate1.onValueChanged.AddListener(delegate { CleanIfBothTerminating(); });
            terminate2.onValueChanged.AddListener(delegate { CleanIfBothTerminating(); });

            if (updateFreqSec > 0)
            {
                InvokeRepeating(nameof(SyncStatus), 0f, updateFreqSec);
            }
            else
            {
                SyncStatus(); // Sync once if frequency is zero or negative.
                enabled = false; // Disable component to stop further updates, matching previous behavior.
            }
        }

        private void CleanIfBothTerminating()
        {
            var left = terminate1.isOn;
            var right = terminate2.isOn;

            if (left && right)
            {
                value?.Dispose();

                PostClean();
            }
        }

        private void PostClean()
        {
            terminate1.enabled = false;
            terminate2.enabled = false;

            Destroy(row.gameObject);
        }


        public virtual void SyncStatus()
        {
            if (value == null) return;

            if (value.IsDisposed)
            {
                PostClean();
                return;
            }

            var vType = value.GetType();
            summary.text = vType.FullName;
            if (detail.isActiveAndEnabled) detail.text = value.ToString();
        }
    }
}