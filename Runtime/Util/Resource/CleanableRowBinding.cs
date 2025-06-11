#nullable enable
using System;
using Autofill;
using MAVLinkAPI.UI.Tables;
using MAVLinkAPI.Util.NullSafety;
using TMPro;
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

        [Serialize] private float updateFreqSec = 0.5f;

        [DoNotSerialize] private Cleanable? underlying;


        public void Bind(Cleanable cleanable)
        {
            underlying = cleanable;
            
            terminate1.onValueChanged.AddListener(delegate { CleanIfBothTerminating(); });
            terminate2.onValueChanged.AddListener(delegate { CleanIfBothTerminating(); });
            
            UpdateStatus(true); // Sync once if frequency is zero or negative.
            
            if (updateFreqSec > 0)
            {
                InvokeRepeating(nameof(UpdateStatusFn), 0f, updateFreqSec);
            }
            else
            {
                enabled = false; // Disable component to stop further updates, matching previous behavior.
            }
        }

        private void CleanIfBothTerminating()
        {
            var left = terminate1.isOn;
            var right = terminate2.isOn;

            if (left && right)
            {
                underlying?.Dispose();

                PostClean();
            }
        }

        private void PostClean()
        {
            terminate1.enabled = false;
            terminate2.enabled = false;

            Destroy(row.gameObject);
        }


        public virtual void UpdateStatus(bool force = false)
        {
            if (underlying == null) return;

            if (underlying.IsDisposed)
            {
                PostClean();
                return;
            }

            summary.text = GetSummary(underlying);
            if (detail.isActiveAndEnabled || force) detail.text = GetDetail(underlying);
        }

        public void UpdateStatusFn()
        {
            UpdateStatus();
        }
        
        public virtual string GetSummary(Cleanable cleanable)
        {
            var vType = underlying.GetType();
            var text = vType.Name;
            return text;
        }

        public virtual string GetDetail(Cleanable cleanable)
        {
            if (cleanable == null) return "Cleanable is null";

            var lifespan = DateTime.UtcNow - cleanable.CreatedAt;
            
            return $@"
Object: {cleanable.ToString()}
- ID: {cleanable.ID}
- Lifespan: {lifespan.TotalSeconds}";
        }
    }
}