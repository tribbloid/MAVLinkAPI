#nullable enable
using MAVLinkAPI.API.Feature;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.SpatialTracking;

namespace MAVLinkAPI.API.UI
{
    public class NavPoseProvider : BasePoseProvider
    {
        public Common.NavigationFeed? ActiveFeed;

        public void Bind(Common.NavigationFeed daemon)
        {
            if (ActiveFeed != null) Unbind();

            ActiveFeed = daemon;
            ActiveFeed.Start();
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
        public override PoseDataFlags GetPoseFromProvider(out Pose output)
        {
            var d = ActiveFeed;
            if (d != null)
            {
                output = new Pose(new Vector3(0, 0, 0), d.LastAttitude.Value);
                return PoseDataFlags.Rotation;
            }

            output = Pose.identity;
            return PoseDataFlags.NoData;
        }
    }
}