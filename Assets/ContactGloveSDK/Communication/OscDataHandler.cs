using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using uOSC;

namespace ContactGloveSDK
{
    public class OscDataHandler : IOscDataHandler
    {
        private readonly IGloveDataController gloveDataController;

        private string oscIPAddress = "127.0.0.1";
        internal const int ReceiveOscPort = 25788;
        internal const int SendOscPort = 25790;

        //TODO: get rid of stating here
        private ControllerInput _controllerInput;

        private DivingStationPairingServer pairingServer;

        // Data which sent at the previous frame
        private readonly IDictionary<OscModes, byte[]> previousData = new Dictionary<OscModes, byte[]>() { };

        internal OscDataHandler(IGloveDataController gloveDataController)
        {
            this.gloveDataController = gloveDataController;

            _controllerInput = new ControllerInput();

            pairingServer = new DivingStationPairingServer(address => { oscIPAddress = address; });
        }

        public string GetOscIpAddress()
        {
            return oscIPAddress;
        }

        public void ClosePairingServer()
        {
            pairingServer.CloseServer();
        }

        /// <summary>
        /// Check if the data is updated and update the data on each OscModes
        /// </summary>
        /// <param name="mode">send mode defined in enum OscModes</param>
        /// <param name="newData">send data in byte[]</param>
        /// <returns></returns>
        public bool IsDataUpdated(OscModes mode, byte[] newData)
        {
            if (!previousData.ContainsKey(mode))
            {
                previousData.Add(mode, newData);
                return true;
            }

            if (!previousData[mode].SequenceEqual(newData))
            {
                previousData[mode] = newData;
                return true;
            }

            return false;
        }

        public void OnDataReceived(Message message)
        {
            if (message.address == OscModes.ReceiveFingerRotRight.ToIpAddress())
            {
                ReceiveFingerRotRight(message.values);
            }
            else if (message.address == OscModes.ReceiveFingerRotLeft.ToIpAddress())
            {
                ReceiveFingerRotLeft(message.values);
            }
            else if (message.address == OscModes.ReceiveControllerInput.ToIpAddress())
            {
                ReceiveControllerInput(message.values);
            }
        }

        public IControllerInput GetControllerInput()
        {
            return _controllerInput;
        }

        private void ReceiveFingerRotRight(object[] rawValues)
        {
            ReceiveFingerRot(rawValues, HandSides.Right);
        }

        private void ReceiveFingerRotLeft(object[] rawValues)
        {
            ReceiveFingerRot(rawValues, HandSides.Left);
        }

        /// <summary>
        /// Receive byte[] finger rotation data from GUI
        /// </summary>
        /// <param name="rawValues"></param>
        /// <param name="hand"></param>
        private void ReceiveFingerRot(object[] rawValues, HandSides hand)
        {
            var values = rawValues[0] as byte[];
            if (values.Length < DataLength.FingerRotBytes)
            {
                Debug.LogError("Finger rotation data length is too short " + values.Length);
                return;
            }

            var floatValues = new float[DataLength.FingerRot];
            values.ToFloatArray(ref floatValues);

            DebugTool.ShowArrayLog(floatValues, "Finger rotation " + hand);

            for (int i = 0; i < DataLength.FingerRot-4; i++)
                gloveDataController.SetFingerRotationAmplitude(hand, (FingerRotationAmplitude_e)i, floatValues[i]);
            // Thumb has different process
            gloveDataController.SetFingerRotationAmplitude(hand, FingerRotationAmplitude_e.ThumbProximal, floatValues[12]/0.66f); // The maximum of the raw value is 0.66
            gloveDataController.SetFingerRotationAmplitude(hand, FingerRotationAmplitude_e.ThumbIntermediate, floatValues[14]);
            gloveDataController.SetFingerRotationAmplitude(hand, FingerRotationAmplitude_e.ThumbDistal, floatValues[15]);
        }

        /// <summary>
        /// Receive Controller Input 
        /// </summary>
        /// <param name="rawValues"></param>
        private void ReceiveControllerInput(object[] rawValues)
        {
            var values = rawValues[0] as byte[];
            
            if (values.Length < DataLength.ControllerInput)
            {
                Debug.LogError("Controller input data length is too short " + values.Length);
                return;
            }

            byte[] controllerInputData = new byte[DataLength.ControllerInput];

            for (int i = 0; i < DataLength.ControllerInput; i++)
                controllerInputData[i] = (byte)values[i];

            _controllerInput.StoreControllerInput(controllerInputData);
        }
    }
}