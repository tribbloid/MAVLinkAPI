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
    public class AhrsNewFeedInput : MonoBehaviour
    {
        /**
         * bind to a lifetime in scene, an <see cref="Ahrs.Feed"/> instance can be created from the last Stream
         *
         * we may add a feature to create a highly-available daemon from all streams in the lifetime
         */
        [Required] public LifetimeBinding lifetimeB = null!;

        [Required] public AhrsPoseProvider poseProvider = null!;

        [Required] public TMP_InputField addressInput = null!;

        [Autofill(AutofillType.Children)] public TMP_Dropdown baudRateInput = null!;
        [Autofill(AutofillType.Children)] public Button newFeedButton = null!;

        private Lifetime Lifetime => lifetimeB.Lifetime;

        void Start()
        {
            baudRateInput.options.Clear();
            IOStream.BaudRates.all.ForEach(baudRate =>
            {
                baudRateInput.options.Add(new TMP_Dropdown.OptionData(baudRate.ToString()));
            });
            baudRateInput.value = IOStream.BaudRates.all.IndexOf(IOStream.BaudRates.Default);

            addressInput.onSubmit.AddListener(_ => BindNewFeed());

            newFeedButton.onClick.AddListener(() => addressInput.onSubmit.Invoke(addressInput.text));
        }

        // public void BindLast()
        // {
        //     // find the last Uplink in the lifetime, create a feed out of it
        //     var lastUplink = Lifetime.CollectByType<Uplink>().Last();
        //
        //     var ahrsFeed = new Ahrs.Feed(Lifetime, lastUplink);
        //
        //     poseProvider.Bind(ahrsFeed);
        // }

        public void BindNewFeed()
        {
            var args = IOStream.ArgsT.Parse(addressInput.text);

            var io = new IOStream(args);
            io.BaudRate = int.Parse(baudRateInput.captionText.text); // TODO: should be autotune 

            var uplink = new DirectUplink(io, null, Lifetime);
            var ahrsFeed = new Ahrs.Feed(Lifetime, uplink);

            poseProvider.Bind(ahrsFeed);
        }
    }
}