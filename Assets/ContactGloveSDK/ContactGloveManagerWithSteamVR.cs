using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using uOSC;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace ContactGloveSDK
{
    public class ContactGloveManagerWithSteamVR : MonoBehaviour, IFlexObjectHandler, IGloveDataController, IContactGloveManager
    {
        private IOscDataHandler oscDataHandler;

        private uOscClient oscClient;

        private HumanPoseHandler humanPoseHandler;

        private IFingerManager fingerManager;

        private IFlexManager flexManager;
        
        private IControllerInput controllerInput;
        
        [SerializeField] private GameObject collisionPrefab;

        /// <summary>
        /// Amplitude of finger rotation. 0: 0 degrees, 1: 90 degrees
        /// Index: FingerRotationAmplitudeRight
        /// </summary>
        private readonly float[,] fingerRotationAmplitude = new float[DataLength.HandLength, DataLength.FingerRot];

        /// <summary>
        /// Use HandPhysics to calculate human pose?
        /// </summary>
        [SerializeField]
        private bool useHandPhysics = false;
        
        [SerializeField, EnumIndex(typeof(FingerHandPhysics_e))]
        private Transform[] fingerHandPhysicsTransformsLeft = new Transform[DataLength.FingerHandPhysics];
        
        [SerializeField, EnumIndex(typeof(FingerHandPhysics_e))]
        private Transform[] fingerHandPhysicsTransformsRight = new Transform[DataLength.FingerHandPhysics];

        private readonly HapticsManager hapticsManager = new HapticsManager();

        public GameObject LeftHand;
        public GameObject RightHand;

        private Hand leftHandManager, rightHandManager;

        private class TmpColliderObjectHandler : IColliderObjectHandler
        {
            private ContactGloveManagerWithSteamVR manager;

            public TmpColliderObjectHandler(ContactGloveManagerWithSteamVR manager)
            {
                this.manager = manager;
            }
            
            public GameObject CreateNewCollisionObject(Vector3 position)
            {
                return Instantiate(manager.collisionPrefab, position, Quaternion.identity);
            }

            public Transform GetBoneTransform(HumanBodyBones bone) => manager.GetBoneTransform(bone);

            public Transform GetFingerTransform(HandSides hand, FingerRotationAmplitude_e bone) => manager.GetFingerTransform(hand, bone);

            public Transform GetHandPhysicsFingerTransform(HandSides hand, FingerHandPhysics_e bone)
            {
                return manager.GetHandPhysicsTransformsRight(hand, bone);
            }
        }

        private void Awake()
        {
            leftHandManager = LeftHand.GetComponent<Hand>();
            rightHandManager = RightHand.GetComponent<Hand>();
            
            oscDataHandler = new OscDataHandler(this);

            fingerManager = new FingerManager(new TmpColliderObjectHandler(this), false, useHandPhysics);

            flexManager = new FlexManager(
                this,
                false,
                useHandPhysics
            );
            
            controllerInput = oscDataHandler.GetControllerInput();
            SetupOsc();
        }

        private void Start()
        {
            flexManager.InitializeFingerRotation(HandSides.Left);
            flexManager.InitializeFingerRotation(HandSides.Right);
        }

        private bool isLeftColliderSetup = false;
        private bool isRightColliderSetup = false;
        
        public static void SetChildLayer(GameObject go, int layer)
        {
            go.layer = layer;
            foreach ( Transform child in go.transform )
            {
                SetChildLayer(child.gameObject, layer);
            }
        }
        
        private void Update()
        {
            flexManager.SetFingerRotationFromAmplitude(HandSides.Left);
            flexManager.SetFingerRotationFromAmplitude(HandSides.Right);

            if (!isLeftColliderSetup && leftHandManager.HasSkeleton())
            {
                fingerManager.MapCollider(HandSides.Left);
                isLeftColliderSetup = true;
            }
            
            if (!isRightColliderSetup && rightHandManager.HasSkeleton())
            {
                fingerManager.MapCollider(HandSides.Right);
                isRightColliderSetup = true;
            }

            SendCollisionData();
        }

        private void OnApplicationQuit()
        {
            // Seems like muscle settings are saved, so reset
            for (int i = 0; i < DataLength.FingerRot; i++)
            {
                flexManager.SetBoneRotation((FingerRotationAmplitude_e)i, 0, HandSides.Right);
                flexManager.SetBoneRotation((FingerRotationAmplitude_e)i, 0, HandSides.Left);
            }

            humanPoseHandler?.Dispose();
            oscDataHandler.ClosePairingServer();
        }

        /// <summary>
        /// Add and setup OSC components
        /// </summary>
        private void SetupOsc()
        {
            // Setup client
            gameObject.AddComponent<uOscClient>();
            oscClient = GetComponent<uOscClient>();
            oscClient.address = oscDataHandler.GetOscIpAddress();
            oscClient.port = OscDataHandler.SendOscPort;

            // Setup server
            gameObject.AddComponent<uOscServer>();
            var server = GetComponent<uOscServer>();
            server.port = OscDataHandler.ReceiveOscPort;
            server.onDataReceived.AddListener(oscDataHandler.OnDataReceived);
        }

        /// <summary>
        /// Send byte[] data to GUI
        /// </summary>
        /// <param name="mode">send mode defined in enum OscModes</param>
        /// <param name="data">(Optional) send data in byte[]</param>
        private void SendDataToGUI(OscModes mode, byte[] data = null)
        {
            oscClient.address = oscDataHandler.GetOscIpAddress();

            // Get the data
            switch (mode)
            {
                // Collision data
                case OscModes.SendCollision:
                    if (data == null)
                        break;

                    // Send the data if it is updated
                    if (oscDataHandler.IsDataUpdated(mode, data))
                    {
                        DebugTool.ShowArrayLog(data, "SendCollisionDataToGUI");
                        oscClient.Send(mode.ToIpAddress(), data);
                    }
                    
                    break;
                
                case OscModes.SendVibration:
                    if (data == null)
                        break;
                    
                    if (oscDataHandler.IsDataUpdated(mode, data))
                    {
                        DebugTool.ShowArrayLog(data, "SendVibrationDataToGUI");
                        oscClient.Send(mode.ToIpAddress(), data);
                    }

                    break;

                // Detect invalid mode
                default:
                    Debug.LogError("SendDataToGUI(): Invalid mode");
                    return;
            }
        }

        /// <summary>
        /// Send Collision Data To GUI
        /// </summary>
        private void SendCollisionData(bool[] collidersFlag = null)
        {
            byte[] data = new byte[] { };
            if (collidersFlag != null)
            {
                fingerManager.ConvertCollisionDataToBytes(ref data, collidersFlag);
            }
            else
            {
                fingerManager.ConvertCollisionDataToBytes(ref data);
            }

            hapticsManager.SetSMAHapticsInBytes(data);

            byte[] hapticsData = hapticsManager.GetHapticsBytes();
            DebugTool.ShowArrayLog(hapticsData, "HapticsData");
            Assert.AreEqual(hapticsData.Length, DataLength.HapticsDataBytes);

            SendDataToGUI(OscModes.SendCollision, hapticsData);
            // SendDataToGUI(OscModes.SendVibration, vibrationData);
        }
        
        /// <summary>
        /// Set finger haptics condition in a certain section.
        /// You must call it with enable=false once after calling with enable=true.
        /// </summary>
        /// <param name="hand">Left or Right</param>
        /// <param name="section">section</param>
        /// <param name="enable">true or false</param>
        public void SetHaptics(HandSides hand, ColliderFinger_e section, bool enable)
        {
            SetHaptics(hand, section, HapticModules.bottomLeft, enable);
            SetHaptics(hand, section, HapticModules.topLeft, enable);
            SetHaptics(hand, section, HapticModules.bottomRight, enable);
            SetHaptics(hand, section, HapticModules.topRight, enable);
        }
        
        public void SetHaptics(HandSides hand, ColliderFinger_e section, HapticModules module, bool enable)
        {
            if(enable)
                hapticsManager.SetHapticsOn(hand, section, module);
            else
                hapticsManager.SetHapticsOff(hand, section, module);
        }
        
        /// <summary>
        /// Set Vibration Style in Glove Body.
        /// </summary>
        /// <param name="hand">Left or Right</param>
        /// <param name="amplitude">0.0 ~ 1.0</param>
        /// <param name="frequency">frequency[Hz]</param>
        /// <param name="duration">duration[s]</param>
        public void SetVibration(HandSides hand, float amplitude, float frequency, float duration)
        {
            // hapticsManager.SetVibration(hand, amplitude, frequency);
            float isRight = hand == HandSides.Right ? 1.0f : 0.0f;
            oscClient.Send(OscModes.SendVibration.ToIpAddress(), isRight, frequency, amplitude, duration);
        }
        
        public VibrationType GetVibration(HandSides hand)
        {
            return hapticsManager.GetVibration(hand);
        }

        public Transform GetBoneTransform(HumanBodyBones bone)
        {
            return null;
        }

        public void GetHumanPose(ref HumanPose humanPose)
        {
            humanPoseHandler.GetHumanPose(ref humanPose);
        }

        public void SetHumanPose(ref HumanPose humanPose)
        {
            humanPoseHandler.SetHumanPose(ref humanPose);
        }

        private Transform GetHandPhysicsTransformsRight(HandSides hand, FingerHandPhysics_e bone)
        {
            SteamVR_Behaviour_Skeleton skelton =
                hand == HandSides.Left ? leftHandManager.skeleton : rightHandManager.skeleton;

            if (ReferenceEquals(null, skelton))
            {
                return null;
            }

            switch (bone)
            {
                case FingerHandPhysics_e.Index:
                    return skelton.indexDistal;
                case FingerHandPhysics_e.Middle:
                    return skelton.middleDistal;
                case FingerHandPhysics_e.Ring:
                    return skelton.ringDistal;
                case FingerHandPhysics_e.Thumb:
                    return skelton.thumbTip;
            }

            return null;
        }

        public Transform GetFingerTransform(HandSides hand, FingerRotationAmplitude_e bone)
        {
            SteamVR_Behaviour_Skeleton skelton =
                hand == HandSides.Left ? leftHandManager.skeleton : rightHandManager.skeleton;
            
            if (ReferenceEquals(null, skelton))
            {
                return null;
            }
            
            switch (bone)
            {
                case FingerRotationAmplitude_e.IndexDistal:
                    return skelton.indexDistal;
                case FingerRotationAmplitude_e.IndexIntermediate:
                    return skelton.indexMiddle;
                case FingerRotationAmplitude_e.IndexProximal:
                    return skelton.indexProximal;
                case FingerRotationAmplitude_e.LittleDistal:
                    return skelton.pinkyDistal;
                case FingerRotationAmplitude_e.LittleIntermediate:
                    return skelton.pinkyMiddle;
                case FingerRotationAmplitude_e.LittleProximal:
                    return skelton.pinkyProximal;
                case FingerRotationAmplitude_e.MiddleDistal:
                    return skelton.middleDistal;
                case FingerRotationAmplitude_e.MiddleIntermediate:
                    return skelton.middleMiddle;
                case FingerRotationAmplitude_e.MiddleProximal:
                    return skelton.middleProximal;
                case FingerRotationAmplitude_e.RingDistal:
                    return skelton.ringDistal;
                case FingerRotationAmplitude_e.RingIntermediate:
                    return skelton.ringMiddle;
                case FingerRotationAmplitude_e.RingProximal:
                    return skelton.ringProximal;
                case FingerRotationAmplitude_e.ThumbAbduction:
                    return skelton.thumbAux;
                case FingerRotationAmplitude_e.ThumbDistal:
                    return skelton.thumbDistal;
                case FingerRotationAmplitude_e.ThumbIntermediate:
                    return skelton.thumbMiddle;
                case FingerRotationAmplitude_e.ThumbProximal:
                    return skelton.thumbProximal;
            }

            return null;
        }

        public void SetFingerRotation(HandSides hand, FingerRotationAmplitude_e bone, Quaternion rot)
        {
            return;
        }

        public float GetFingerRotationAmplitude(HandSides hand, FingerRotationAmplitude_e index)
        {
            return fingerRotationAmplitude[(int)hand, (int)index];
        }

        public void SetFingerRotationAmplitude(HandSides hand, FingerRotationAmplitude_e index, float amplitude)
        {
            fingerRotationAmplitude[(int)hand, (int)index] = amplitude;
        }

        public bool GetControllerInput(HandSides hand, ControllerBoolInputType type)
        {
            return controllerInput.GetControllerInput(hand, type);
        }

        public float GetControllerInput(HandSides hand, ControllerFloatInputType type)
        {
            return controllerInput.GetControllerInput(hand, type);
        }
    
        public void AddOnControllerInputHandler(HandSides hand, ControllerBoolInputType type, ControllerInputHandler handler)
        {
            controllerInput.AddOnControllerInputHandler(hand, type, handler);
        }

        public void AddOffControllerInputHandler(HandSides hand, ControllerBoolInputType type, ControllerInputHandler handler)
        {
            controllerInput.AddOffControllerInputHandler(hand, type, handler);
        }
    }
}