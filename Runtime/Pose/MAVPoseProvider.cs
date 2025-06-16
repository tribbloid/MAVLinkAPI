#nullable enable
using System;
using System.Threading.Tasks;
using MAVLinkAPI.API.Feature;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.SpatialTracking;

namespace MAVLinkAPI.Pose
{
    public class MAVPoseProvider : BasePoseProvider
    {
        private Ahrs.Daemon? _feed;

        private void OnDestroy()
        {
            Unbind();
        }

        public void Bind(Ahrs.Daemon daemon)
        {
            lock (this)
            {
                _feed = daemon;
            }

            Task.Run(() =>
                {
                    try
                    {
                        _feed.Start();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            );
        }

        public void Unbind()
        {
            lock (this)
            {
                _feed?.Dispose();
                _feed = null;
            }
        }

        // Update Pose
        public override PoseDataFlags GetPoseFromProvider(out UnityEngine.Pose output)
        {
            var d = _feed;
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