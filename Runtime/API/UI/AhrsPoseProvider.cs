#nullable enable
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Autofill;
using MAVLinkAPI.API.Feature;
using MAVLinkAPI.Routing;
using MAVLinkAPI.Util.NullSafety;
using MAVLinkAPI.Util.Resource;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;

namespace MAVLinkAPI.API.UI
{
    public class AhrsPoseProvider : BasePoseProvider
    {
        public Ahrs.Feed? ActiveFeed;

        public void Bind(Ahrs.Feed daemon)
        {
            ActiveFeed = daemon;

            try
            {
                ActiveFeed.Start();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void Unbind()
        {
            lock (this)
            {
                ActiveFeed?.Dispose();
                ActiveFeed = null;
            }
        }


        private void OnDestroy()
        {
            Unbind();
        }

        // Update Pose
        public override PoseDataFlags GetPoseFromProvider(out UnityEngine.Pose output)
        {
            var d = ActiveFeed;
            if (d != null)
            {
                output = new UnityEngine.Pose(new Vector3(0, 0, 0), d.Attitude);
                return PoseDataFlags.Rotation;
            }

            output = UnityEngine.Pose.identity;
            return PoseDataFlags.NoData;
        }
    }
}