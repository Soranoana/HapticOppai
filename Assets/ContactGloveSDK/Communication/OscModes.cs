using UnityEngine;

namespace ContactGloveSDK
{
    // Definition of send/receive modes
    // 0 ~ 9: send; 10 ~ : receive
    public enum OscModes
    {
        SendCollision = 0,
        SendVibration = 1,
        ReceiveFingerRotRight = 10,
        ReceiveFingerRotLeft = 11,
        ReceiveControllerInput = 12
    }

    internal static class OscModesExt
    {
        internal static string ToIpAddress(this OscModes mode)
        {
            switch (mode)
            {
                case OscModes.SendCollision:
                    return "/DivingStation/CollisionSignal";
                case OscModes.SendVibration:
                    return "/DivingStation/HapticVibration";
                case OscModes.ReceiveFingerRotRight:
                    return "/DivingStation/FingerRotRight";
                case OscModes.ReceiveFingerRotLeft:
                    return "/DivingStation/FingerRotLeft";
                case OscModes.ReceiveControllerInput:
                    return "/DivingStation/ControllerInput";
            }

            Debug.LogError("Switch is not exhaustive");
            return "";
        }
    }
}