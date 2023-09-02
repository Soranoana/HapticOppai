using System.Linq;
using System;
using UnityEngine;

namespace ContactGloveSDK
{
    public delegate void ControllerInputHandler();
    
    internal class ControllerInput : IControllerInput
    {

        private static int GetEnumSize(Type t)
        {
            return Enum.GetNames(t).Length;
        }

        private ControllerInputHandler[,] OnControllerInputHandler
            = new ControllerInputHandler[GetEnumSize(typeof(HandSides)), GetEnumSize(typeof(ControllerBoolInputType))];
        
        private ControllerInputHandler[,] OffControllerInputHandler
            = new ControllerInputHandler[GetEnumSize(typeof(HandSides)), GetEnumSize(typeof(ControllerBoolInputType))];

        public ControllerInput()
        {
            left = new ControllerData(HandSides.Left);
            right = new ControllerData(HandSides.Right);
        }

        class ControllerData
        {
            public readonly HandSides hand;
            public readonly bool A, B, Home, JoystickButton, TrackpadTouch;
            public readonly float JoystickX, JoystickY, Trigger, GripValue, GripForce;

            public ControllerData(HandSides hand)
            {
                this.hand = hand;
            }

            public ControllerData(byte[] value)
            {
                this.hand = value[0] == (byte)HandSides.Left ? HandSides.Left : HandSides.Right;
                this.A = value[(int)ControllerBoolInputType.A + 1] == 1;
                this.B = value[(int)ControllerBoolInputType.B + 1] == 1;
                this.Home = value[(int)ControllerBoolInputType.Home + 1] == 1;
                this.JoystickButton = value[(int)ControllerBoolInputType.JoystickButton + 1] == 1;
                this.TrackpadTouch = value[(int)ControllerBoolInputType.TrackpadTouch + 1] == 1;
                var controllerValues = new float[7];
                value.Skip(11).ToArray().ToFloatArray(ref controllerValues);
                this.Trigger = controllerValues[0];
                this.GripValue = controllerValues[1];
                this.GripForce = controllerValues[2];
                this.JoystickX = controllerValues[3];
                this.JoystickY = controllerValues[4];
            }

            public bool GetControllerInput(ControllerBoolInputType type)
            {
                switch (type)
                {
                    case ControllerBoolInputType.A:
                        return A;
                    case ControllerBoolInputType.B:
                        return B;
                    case ControllerBoolInputType.Home:
                        return Home;
                    case ControllerBoolInputType.JoystickButton:
                        return JoystickButton;
                    case ControllerBoolInputType.TrackpadTouch:
                        return TrackpadTouch;
                    default:
                        throw new NotImplementedException();
                }
            }
            
            public float GetControllerInput(ControllerFloatInputType type)
            {
                switch (type)
                {
                    case ControllerFloatInputType.Trigger:
                        return Trigger;
                    case ControllerFloatInputType.GripForce:
                        return GripForce;
                    case ControllerFloatInputType.GripValue:
                        return GripValue;
                    case ControllerFloatInputType.JoystickX:
                        return JoystickX;
                    case ControllerFloatInputType.JoystickY:
                        return JoystickY;
                    default:
                        throw new NotImplementedException();
                }
            }
            
            public void showControllerInput()
            {
                string suffix = HandSides.Left == hand ? "L" : "R";
                Debug.Log($"A_{suffix}: {A}");
                Debug.Log($"B_{suffix}: {B}");
                Debug.Log($"Home_{suffix}: {Home}");
                Debug.Log($"JoystickButton_{suffix}: {JoystickButton}");
                Debug.Log($"TrackpadTouch_{suffix}: {TrackpadTouch}");
                Debug.Log($"Trigger_{suffix}: {Trigger}");
                Debug.Log($"GripForce_{suffix}: {GripForce}");
                Debug.Log($"GripValue_{suffix}: {GripValue}");
                Debug.Log($"JoystickX_{suffix}: {JoystickX}");
                Debug.Log($"JoystickY_{suffix}: {JoystickY}");
            }
        }

        private ControllerData left, right;

        public void StoreControllerInput(byte[] value)
        {
            ControllerData nowData = new ControllerData(value);
            ControllerData preData = GetControllerData(nowData.hand);
            
            if (nowData.hand == HandSides.Left)
            {
                left = nowData;
            }
            else
            {
                right = nowData;
            }

            foreach (ControllerBoolInputType type in Enum.GetValues(typeof(ControllerBoolInputType)))
            {
                if (nowData.GetControllerInput(type) && !preData.GetControllerInput(type))
                {
                    OnControllerInputHandler[(int)nowData.hand, (int)type]?.Invoke();
                }
                if (!nowData.GetControllerInput(type) && preData.GetControllerInput(type))
                {
                    OffControllerInputHandler[(int)nowData.hand, (int)type]?.Invoke();
                }
            }
            
        }
        public void showControllerInput()
        {
            left.showControllerInput();
            right.showControllerInput();
        }

        private ControllerData GetControllerData(HandSides hand)
        {
            return hand == HandSides.Left ? left : right;
        }

        public bool GetControllerInput(HandSides hand, ControllerBoolInputType type)
        {
            return GetControllerData(hand).GetControllerInput(type);
        }

        public float GetControllerInput(HandSides hand, ControllerFloatInputType type)
        {
            return GetControllerData(hand).GetControllerInput(type);
        }

        public void AddOnControllerInputHandler(HandSides hand, ControllerBoolInputType type, ControllerInputHandler handler)
        {
            OnControllerInputHandler[(int)hand, (int)type] += handler;
        }

        public void AddOffControllerInputHandler(HandSides hand, ControllerBoolInputType type, ControllerInputHandler handler)
        {
            OffControllerInputHandler[(int)hand, (int)type] += handler;
        }
    };
}