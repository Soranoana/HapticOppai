using UnityEngine;
using UnityEngine.Serialization;

namespace ContactGloveSDK
{
    public class ViveTrackerManager : MonoBehaviour
    {
        public HandSides Side;

        // private EasyLazyLibrary.EasyOpenVRUtil.Transform viveTrackerOffset;
        private VRTransform viveTrackerOffset = new VRTransform();
        private Vector3 initialWristPosition;
        private Vector3 wristToPalmInitial = new Vector3(.5f, 0, 0);
        private Quaternion initialWristRotation;
        private Quaternion hmdOffsetRotation = Quaternion.identity;

        // the transform of hand model
        public Transform HandModel;

        #region VRTransformClass

        /// <summary>
        /// Transform class for vive tracker and hmd
        /// </summary>
        private class VRTransform
        {
            public Vector3 position;
            public Quaternion rotation;

            public VRTransform()
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
            }

            /// <summary>
            /// Initialization for HMD
            /// </summary>
            /// <param name="transform"></param>
            public VRTransform(Transform transform)
            {
                position = transform.position;
                rotation = transform.rotation * Quaternion.AngleAxis(90, Vector3.right) *
                           Quaternion.AngleAxis(-90, Vector3.up);
            }

            /// <summary>
            /// Initialization for vive tracker
            /// </summary>
            /// <param name="transform"></param>
            /// <param name="hand">left or right</param>
            public VRTransform(Transform transform, HandSides hand)
            {
                position = transform.position;

                // The calculation of the rotation below is set assuming the default rotation of hand models are like the image below
                // https://gyazo.com/a8b59695303cdc9178e815b57608e312
                if (hand == HandSides.Right)
                {
                    rotation = /*Quaternion.AngleAxis(90, Vector3.up)*/ transform.rotation * Quaternion.AngleAxis(90, Vector3.right) *
                               Quaternion.AngleAxis(-90, Vector3.up);
                    
                }
                else
                {
                    rotation = transform.rotation * Quaternion.AngleAxis(90, Vector3.right) *
                               Quaternion.AngleAxis(-90, Vector3.up)
                               * Quaternion.AngleAxis(180, Vector3.forward);
                }
            }

            /// <summary>
            /// Extract the yaw rotation from the quaternion.
            /// </summary>
            /// <returns>Returns which yaw angle the transform is looking at.</returns>
            public Quaternion GetYawOnlyRotation()
            {
                Vector3 v = rotation * Vector3.forward;
                float yaw = Mathf.Atan2(v.z, v.x) * Mathf.Rad2Deg;
                return Quaternion.AngleAxis(-yaw, Vector3.up);
            }

            /// <summary>
            /// Extract the yaw rotation from the quaternion, then return the quaternion that reverts the yaw rotation.
            /// </summary>
            /// <returns> the quaternion that reverts the yaw rotation</returns>
            public Quaternion GetRevertedYawOnlyRotation()
            {
                Vector3 v = rotation * Vector3.forward;
                float yaw = Mathf.Atan2(v.z, v.x) * Mathf.Rad2Deg;
                return Quaternion.AngleAxis(yaw, Vector3.up);
            }
        }

        #endregion
        void Start()
        {

            initialWristRotation = HandModel.rotation;
        }

        void Update()
        {
            SetPoseFromViveTracker();
        }


        /// <summary>
        /// Set hand pose from vive tracker
        /// </summary>
        void SetPoseFromViveTracker()
        {
            VRTransform viveTracker = new VRTransform(this.transform, Side);

            // The vector of wrist to palm. This is if vive tracker is not mounted on the center of hand.
            Vector3 wristToPalm = initialWristRotation * Quaternion.Inverse(viveTrackerOffset.rotation) *
                                  viveTracker.rotation * wristToPalmInitial;

            Vector3 rightWristPosition = hmdOffsetRotation * viveTrackerOffset.rotation *
                                         (viveTracker.position - viveTrackerOffset.position) ;

            HandModel.position = rightWristPosition ;
            HandModel.rotation = initialWristRotation * hmdOffsetRotation * viveTrackerOffset.rotation * viveTracker.rotation;
        }
    }
}