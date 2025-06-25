#nullable enable
using System;
using System.Collections;
using System.Threading.Tasks;
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
    public class ServiceRowController : MonoBehaviour
    {
        [Autofill] public TableRow row = null!;

        [Required] public TextMeshProUGUI summary = null!;
        [Required] public TextMeshProUGUI detail = null!;

        // if both are ticked the instance will be terminated
        [Required] public Toggle terminate1 = null!;
        [Required] public Toggle terminate2 = null!;

        [Serialize] private readonly float _updateFreqSec = 1.5f;

        [Tooltip("The default icon to display when no specific icon is found for the cleanable's type.")] [Required]
        public MutableComponent<RectTransform> icon = null!;

        [SerializeField] public SerializedDict<string, RectTransform> iconTemplates = new();

        [DoNotSerialize] private Cleanable? _underlying;

        private void SetIcon(Cleanable cleanable)
        {
            var typeName = cleanable.GetType().Name;

            if (iconTemplates.Dictionary?.TryGetValue(typeName, out var template) == true && template != null)
                icon.CopyToReplace(template);
            else
                Debug.Log($"unknown type {typeName}");
        }

        private async Task CleanIfBothTerminating()
        {
            var left = terminate1.isOn;
            var right = terminate2.isOn;

            if (left && right)
            {
                terminate1.interactable = false;
                terminate2.interactable = false;

                await Task.Run(() =>
                    {
                        _underlying?.Dispose();

                        Destroy(row.gameObject);
                    }
                );
            }
        }

        public void Bind(Cleanable cleanable)
        {
            _underlying = cleanable;

            StartCoroutine(StartUpdatingStatusAsync()); // is coroutine overkill?
        }

        private IEnumerator StartUpdatingStatusAsync()
        {
            SetIcon(_underlying!);

            terminate1.onValueChanged.AddListener(delegate { CleanIfBothTerminating(); });
            terminate2.onValueChanged.AddListener(delegate { CleanIfBothTerminating(); });

            yield return new WaitForEndOfFrame();

            UpdateStatus(true); // Sync once if frequency is zero or negative.

            if (_updateFreqSec > 0)
            {
                InvokeRepeating(nameof(UpdateStatusFn), 0f, _updateFreqSec);
            }
            else
            {
                UpdateStatus(true); // Sync once if frequency is zero or negative.
                enabled = false; // Disable component to stop further updates, matching previous behavior.
            }
        }

        public virtual void UpdateStatus(bool force = false)
        {
            if (_underlying == null) return;

            if (_underlying.IsDisposed)
            {
                terminate1.interactable = false;
                terminate2.interactable = false;

                Destroy(row.gameObject);
                return;
            }

            try
            {
                summary.text = _underlying.GetStatusSummary();

                if (detail.isActiveAndEnabled || force)
                    try
                    {
                        detail.text = string.Join("\n", _underlying.GetStatusDetail());
                    }
                    catch (Exception ex)
                    {
                        detail.text = $"<error> : {ex.Message}";
                    }
            }
            catch (Exception ex)
            {
                summary.text = $"<error> : {ex.GetType()}";
            }
        }

        public void UpdateStatusFn()
        {
            UpdateStatus();
        }
    }
}