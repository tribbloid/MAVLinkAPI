#nullable enable
using System;
using Autofill;
using MAVLinkAPI.UI;
using MAVLinkAPI.UI.Tables;
using MAVLinkAPI.Util.NullSafety;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MAVLinkAPI.Util.Resource.UI
{
    public class CleanableRowBinding : MonoBehaviour
    {
        [Autofill] public TableRow row = null!;

        [Required] public TextMeshProUGUI summary = null!;
        [Required] public TextMeshProUGUI detail = null!;

        // if both are ticked the instance will be terminated
        [Required] public Toggle terminate1 = null!;
        [Required] public Toggle terminate2 = null!;

        [Serialize] private readonly float _updateFreqSec = 0.5f;

        [Tooltip("The default icon to display when no specific icon is found for the cleanable's type.")] [Required]
        public MutableComponent<Graphic> icon = null!;

        [SerializeField] public SerializedDict<string, Graphic> iconTemplates = new();

        [DoNotSerialize] private Cleanable? _underlying;

        public void Bind(Cleanable cleanable)
        {
            _underlying = cleanable;

            UpdateIcon(cleanable);

            terminate1.onValueChanged.AddListener(delegate { CleanIfBothTerminating(); });
            terminate2.onValueChanged.AddListener(delegate { CleanIfBothTerminating(); });

            UpdateStatus(true); // Sync once if frequency is zero or negative.

            if (_updateFreqSec > 0)
            {
                InvokeRepeating(nameof(UpdateStatusFn), 0f, _updateFreqSec);
            }
            else
            {
                enabled = false; // Disable component to stop further updates, matching previous behavior.
            }
        }


        private void UpdateIcon(Cleanable cleanable)
        {
            var typeName = cleanable.GetType().Name;

            if (iconTemplates.Dictionary?.TryGetValue(typeName, out var template) == true && template != null)
            {
                icon.CopyToReplace(template);
            }
        }

        private void CleanIfBothTerminating()
        {
            var left = terminate1.isOn;
            var right = terminate2.isOn;

            if (left && right)
            {
                _underlying?.Dispose();

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
            if (_underlying == null) return;

            if (_underlying.IsDisposed)
            {
                PostClean();
                return;
            }

            try
            {
                summary.text = _underlying.GetStatusSummary();

                if (detail.isActiveAndEnabled || force)
                    detail.text = string.Join(
                        "\n", _underlying.GetStatusDetail()
                    );
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        public void UpdateStatusFn()
        {
            UpdateStatus();
        }
    }
}