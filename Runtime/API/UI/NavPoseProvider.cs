#nullable enable
using System;
using MAVLinkAPI.API.Feature;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.SpatialTracking;

namespace MAVLinkAPI.API.UI
{
    public class NavPoseProvider : BasePoseProvider
    {
        public bool verboseLogging = false;

        [NonSerialized] public Common.NavigationFeed? ActiveFeed;

        public void Connect(Common.NavigationFeed daemon)
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
                var qRaw = d.LastAttitude.Value;
                output = new Pose(new Vector3(0, 0, 0), qRaw);

                if (verboseLogging)
                    Debug.Log($"update from XR (Euler): {qRaw.x}, {qRaw.y}, {qRaw.z}, {qRaw.w}");

                return PoseDataFlags.Rotation;
            }

            output = Pose.identity;
            return PoseDataFlags.NoData;
        }
    }
}