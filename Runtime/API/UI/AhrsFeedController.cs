#nullable enable
using System;
using System.Threading.Tasks;
using Autofill;
using MAVLinkAPI.API.Feature;
using MAVLinkAPI.Routing;
using MAVLinkAPI.Util.NullSafety;
using MAVLinkAPI.Util.Resource;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MAVLinkAPI.API.UI
{
    public class AhrsFeedController : MonoBehaviour
    {
        /**
         * bind to a lifetime in scene, an <see cref="Ahrs.Feed"/> instance can be created from the last Stream
         *
         * we may add a feature to create a highly-available daemon from all streams in the lifetime
         */
        [Required] public LifetimeBinding lifetimeBinding = null!;

        [Required] public AhrsPoseProvider poseProvider = null!;

        [Required] public TMP_InputField addressInput = null!;

        [Autofill(AutofillType.Children)] public TMP_Dropdown baudRateInput = null!;
        [Autofill(AutofillType.Children)] public Button newFeedButton = null!;

        private Lifetime Lifetime => lifetimeBinding.Lifetime;

        private void Start()
        {
            baudRateInput.options.Clear();
            IOStream.BaudRates.all.ForEach(baudRate =>
            {
                baudRateInput.options.Add(new TMP_Dropdown.OptionData(baudRate.ToString()));
            });
            baudRateInput.value = IOStream.BaudRates.all.IndexOf(IOStream.BaudRates.Default);

            addressInput.onSubmit.AddListener(address => BindInputAsync(address));

            newFeedButton.onClick.AddListener(() => addressInput.onSubmit.Invoke(addressInput.text));
        }

        private void BindInputAsync(string address)
        {
            var args = IOStream.ArgsT.Parse(address);


            Task.Run(() =>
            {
                Uplink? uplink = null;
                Ahrs.Feed? feed = null;
                try
                {
                    var io = new IOStream(args);
                    uplink = new DirectUplink(io, null, Lifetime);
                    io.BaudRate = int.Parse(baudRateInput.captionText.text); // TODO: should be autotune 

                    feed = Ahrs.Feed.OfUplink(Lifetime, uplink);

                    poseProvider.Bind(feed);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    feed?.Dispose();
                    uplink?.Dispose();
                }
            });
        }

        public void PrintInfo() // will print very long stats in the console
        {
            if (poseProvider.ActiveFeed == null) return;

            var uplinks = poseProvider.ActiveFeed.Updater.Sources.Keys;

            foreach (var uplink in uplinks)
            {
                var info = uplink.ToString() + "\n" + uplink.Metric.Histogram.ToString();

                Debug.Log(info);
            }
        }
    }
}