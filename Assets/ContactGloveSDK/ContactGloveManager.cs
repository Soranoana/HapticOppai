using UnityEngine;
using UnityEngine.Assertions;
using uOSC;

namespace ContactGloveSDK
{
    public class ContactGloveManager : MonoBehaviour, IFlexObjectHandler, IGloveDataController, IContactGloveManager
    {
        private IOscDataHandler oscDataHandler;

        private uOscClient oscClient;

        private HumanPoseHandler humanPoseHandler;

        private IFingerManager fingerManager;

        private IFlexManager flexManager;

        private IControllerInput controllerInput;

        [SerializeField] private Animator animator;

        [SerializeField] private GameObject collisionPrefab;

        /// <summary>
        /// Amplitude of finger rotation. 0: 0 degrees, 1: 90 degrees
        /// Index: FingerRotationAmplitudeRight
        /// </summary>
        private readonly float[,] fingerRotationAmplitude = new float[DataLength.HandLength, DataLength.FingerRot];

        /// <summary>
        /// Whether to use Animator or not. If true, fingerTransforms will be ignored.
        /// </summary>
        [SerializeField, Header("Flex"), Space(10),]
        private bool useAnimator = false;

        /// <summary>
        /// Use HandPhysics to calculate human pose?
        /// </summary>
        [SerializeField]
        private bool useHandPhysics = false;

        /// <summary>
        /// Finger Transforms. Used when useAnimator is false
        /// </summary>
        [SerializeField, EnumIndex(typeof(FingerRotationAmplitude_e))]
        private Transform[] fingerTransformsRight = new Transform[DataLength.FingerBone];

        [SerializeField, EnumIndex(typeof(FingerRotationAmplitude_e))]
        private Transform[] fingerTransformsLeft = new Transform[DataLength.FingerBone];

        [SerializeField, EnumIndex(typeof(FingerHandPhysics_e))]
        private Transform[] fingerHandPhysicsTransformsLeft = new Transform[DataLength.FingerHandPhysics];

        [SerializeField, EnumIndex(typeof(FingerHandPhysics_e))]
        private Transform[] fingerHandPhysicsTransformsRight = new Transform[DataLength.FingerHandPhysics];

        private readonly HapticsManager hapticsManager = new HapticsManager();

        private class TmpColliderObjectHandler : IColliderObjectHandler
        {
            private ContactGloveManager manager;

            public TmpColliderObjectHandler(ContactGloveManager manager)
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
                if (hand == HandSides.Right)
                    return manager.fingerHandPhysicsTransformsRight[(int)bone];
                else
                    return manager.fingerHandPhysicsTransformsLeft[(int)bone];
            }
        }

        private void Awake()
        {
            oscDataHandler = new OscDataHandler(this);

            fingerManager = new FingerManager(new TmpColliderObjectHandler(this), useAnimator, useHandPhysics);

            flexManager = new FlexManager(
                this,
                useAnimator,
                useHandPhysics
            );

            SetupOsc();
            
            controllerInput = oscDataHandler.GetControllerInput();
            fingerManager.MapCollider();
        }

        private void Start()
        {
            if (!useAnimator)
            {
                flexManager.InitializeFingerRotation(HandSides.Left);
                flexManager.InitializeFingerRotation(HandSides.Right);
            }
            else
            {
                humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            }


        }
        
        private void Update()
        {
            flexManager.SetFingerRotationFromAmplitude(HandSides.Left);
            flexManager.SetFingerRotationFromAmplitude(HandSides.Right);
            
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
        internal void SendCollisionData(bool[] collidersFlag = null)
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

            // byte[] vibrationData = hapticsManager.GetVibrationBytes();
            // DebugTool.ShowArrayLog(vibrationData, "VibrationData");
            // Assert.AreEqual(vibrationData.Length, DataLength.VibrationLength);

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

        public void SetFingerHapticsStrength(HandSides hand, int strength)
        {
            hapticsManager.SetFingerHapticsStrength(hand, strength);
        }

        public bool GetHaptics(HandSides hand, ColliderFinger_e section, HapticModules collider)
        {
            return hapticsManager.GetHaptics(hand, section, collider);
        }

        public VibrationType GetVibration(HandSides hand)
        {
            return hapticsManager.GetVibration(hand);
        }

        public int GetFingerHapticsStrength(HandSides hand)
        {
            return hapticsManager.GetFingerHapticsStrength(hand);
        }

        public Transform GetBoneTransform(HumanBodyBones bone)
        {
            return animator.GetBoneTransform(bone);
        }

        public void GetHumanPose(ref HumanPose humanPose)
        {
            humanPoseHandler.GetHumanPose(ref humanPose);
        }

        public void SetHumanPose(ref HumanPose humanPose)
        {
            humanPoseHandler.SetHumanPose(ref humanPose);
        }

        public Transform GetFingerTransform(HandSides hand, FingerRotationAmplitude_e bone)
        {
            if (hand == HandSides.Right)
                return fingerTransformsRight[(int)bone];
            else
                return fingerTransformsLeft[(int)bone];
        }

        public void SetFingerRotation(HandSides hand, FingerRotationAmplitude_e bone, Quaternion rot)
        {
            if (hand == HandSides.Right)
                fingerTransformsRight[(int)bone].localRotation = rot;
            else
                fingerTransformsLeft[(int)bone].localRotation = rot;
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