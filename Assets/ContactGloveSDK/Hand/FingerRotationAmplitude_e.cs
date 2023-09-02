using UnityEngine;

namespace ContactGloveSDK
{
    /// <summary>
    /// Enum of finger rotation itself. Index: order of finger rotation amplitude.
    /// </summary>
    public enum FingerRotationAmplitude_e : int
    {
        LittleProximal = 0,
        LittleIntermediate = 1,
        LittleDistal = 2,
        RingProximal = 3,
        RingIntermediate = 4,
        RingDistal = 5,
        MiddleProximal = 6,
        MiddleIntermediate = 7,
        MiddleDistal = 8,
        IndexProximal = 9,
        IndexIntermediate = 10,
        IndexDistal = 11,
        ThumbProximal = 12,
        ThumbIntermediate = 13,
        ThumbDistal = 14,
        ThumbAbduction = 15
    }

    internal static class FingerRotationAmplitude_e_Ext
    {
        internal static int? ToMuscleIndex(this FingerRotationAmplitude_e bone, HandSides hand)
        {
            if (hand == HandSides.Right)
            {
                switch (bone)
                {
                    case FingerRotationAmplitude_e.IndexProximal:
                        return 79;
                    case FingerRotationAmplitude_e.IndexIntermediate:
                        return 81;
                    case FingerRotationAmplitude_e.IndexDistal:
                        return 82;
                    case FingerRotationAmplitude_e.MiddleProximal:
                        return 83;
                    case FingerRotationAmplitude_e.MiddleIntermediate:
                        return 85;
                    case FingerRotationAmplitude_e.MiddleDistal:
                        return 86;
                    case FingerRotationAmplitude_e.RingProximal:
                        return 87;
                    case FingerRotationAmplitude_e.RingIntermediate:
                        return 89;
                    case FingerRotationAmplitude_e.RingDistal:
                        return 90;
                    case FingerRotationAmplitude_e.LittleProximal:
                        return 91;
                    case FingerRotationAmplitude_e.LittleIntermediate:
                        return 93;
                    case FingerRotationAmplitude_e.LittleDistal:
                        return 94;
                    case FingerRotationAmplitude_e.ThumbProximal:
                        return 75;
                    case FingerRotationAmplitude_e.ThumbIntermediate:
                        return 77;
                    case FingerRotationAmplitude_e.ThumbDistal:
                        return 78;
                    default:
                        return null;
                }
            }

            switch (bone)
            {
                case FingerRotationAmplitude_e.IndexProximal:
                    return 59;
                case FingerRotationAmplitude_e.IndexIntermediate:
                    return 61;
                case FingerRotationAmplitude_e.IndexDistal:
                    return 62;
                case FingerRotationAmplitude_e.MiddleProximal:
                    return 63;
                case FingerRotationAmplitude_e.MiddleIntermediate:
                    return 65;
                case FingerRotationAmplitude_e.MiddleDistal:
                    return 66;
                case FingerRotationAmplitude_e.RingProximal:
                    return 67;
                case FingerRotationAmplitude_e.RingIntermediate:
                    return 69;
                case FingerRotationAmplitude_e.RingDistal:
                    return 70;
                case FingerRotationAmplitude_e.LittleProximal:
                    return 71;
                case FingerRotationAmplitude_e.LittleIntermediate:
                    return 73;
                case FingerRotationAmplitude_e.LittleDistal:
                    return 74;
                case FingerRotationAmplitude_e.ThumbProximal:
                    return 55;
                case FingerRotationAmplitude_e.ThumbIntermediate:
                    return 57;
                case FingerRotationAmplitude_e.ThumbDistal:
                    return 58;
                default:
                    return null;
            }
        }

        internal static HumanBodyBones ToHumanBodyBones(this FingerRotationAmplitude_e finger, HandSides hand)
        {
            if (hand == HandSides.Right)
            {
                switch (finger)
                {
                    case FingerRotationAmplitude_e.IndexDistal:
                        return HumanBodyBones.RightIndexDistal;
                    case FingerRotationAmplitude_e.IndexIntermediate:
                        return HumanBodyBones.RightIndexIntermediate;
                    case FingerRotationAmplitude_e.IndexProximal:
                        return HumanBodyBones.RightIndexProximal;
                    case FingerRotationAmplitude_e.MiddleDistal:
                        return HumanBodyBones.RightMiddleDistal;
                    case FingerRotationAmplitude_e.MiddleIntermediate:
                        return HumanBodyBones.RightMiddleIntermediate;
                    case FingerRotationAmplitude_e.MiddleProximal:
                        return HumanBodyBones.RightMiddleProximal;
                    case FingerRotationAmplitude_e.RingDistal:
                        return HumanBodyBones.RightRingDistal;
                    case FingerRotationAmplitude_e.RingIntermediate:
                        return HumanBodyBones.RightRingIntermediate;
                    case FingerRotationAmplitude_e.RingProximal:
                        return HumanBodyBones.RightRingProximal;
                    case FingerRotationAmplitude_e.LittleDistal:
                        return HumanBodyBones.RightLittleDistal;
                    case FingerRotationAmplitude_e.LittleIntermediate:
                        return HumanBodyBones.RightLittleIntermediate;
                    case FingerRotationAmplitude_e.LittleProximal:
                        return HumanBodyBones.RightLittleProximal;
                    case FingerRotationAmplitude_e.ThumbDistal:
                        return HumanBodyBones.RightThumbDistal;
                    case FingerRotationAmplitude_e.ThumbIntermediate:
                        return HumanBodyBones.RightThumbIntermediate;
                    case FingerRotationAmplitude_e.ThumbProximal:
                        return HumanBodyBones.RightThumbProximal;
                    default:
                        Debug.Log("Unknown finger" + finger);
                        return HumanBodyBones.LastBone;
                }
            }
            else if (hand == HandSides.Left)
            {
                switch (finger)
                {
                    case FingerRotationAmplitude_e.IndexDistal:
                        return HumanBodyBones.LeftIndexDistal;
                    case FingerRotationAmplitude_e.IndexIntermediate:
                        return HumanBodyBones.LeftIndexIntermediate;
                    case FingerRotationAmplitude_e.IndexProximal:
                        return HumanBodyBones.LeftIndexProximal;
                    case FingerRotationAmplitude_e.MiddleDistal:
                        return HumanBodyBones.LeftMiddleDistal;
                    case FingerRotationAmplitude_e.MiddleIntermediate:
                        return HumanBodyBones.LeftMiddleIntermediate;
                    case FingerRotationAmplitude_e.MiddleProximal:
                        return HumanBodyBones.LeftMiddleProximal;
                    case FingerRotationAmplitude_e.RingDistal:
                        return HumanBodyBones.LeftRingDistal;
                    case FingerRotationAmplitude_e.RingIntermediate:
                        return HumanBodyBones.LeftRingIntermediate;
                    case FingerRotationAmplitude_e.RingProximal:
                        return HumanBodyBones.LeftRingProximal;
                    case FingerRotationAmplitude_e.LittleDistal:
                        return HumanBodyBones.LeftLittleDistal;
                    case FingerRotationAmplitude_e.LittleIntermediate:
                        return HumanBodyBones.LeftLittleIntermediate;
                    case FingerRotationAmplitude_e.LittleProximal:
                        return HumanBodyBones.LeftLittleProximal;
                    case FingerRotationAmplitude_e.ThumbDistal:
                        return HumanBodyBones.LeftThumbDistal;
                    case FingerRotationAmplitude_e.ThumbIntermediate:
                        return HumanBodyBones.LeftThumbIntermediate;
                    case FingerRotationAmplitude_e.ThumbProximal:
                        return HumanBodyBones.LeftThumbProximal;
                    
                    default:
                        Debug.Log("Unknown finger" + finger);
                        return HumanBodyBones.LastBone;
                }
            }

            Debug.Log("Finger not found");
            return HumanBodyBones.LastBone;
        }


        internal static float GetMaximumAmplitude(this FingerRotationAmplitude_e bone)
        {
            float maximumAmplitude = 0;
            switch (bone)
            {
                case FingerRotationAmplitude_e.ThumbIntermediate:
                    maximumAmplitude = 55;
                    break;
                case FingerRotationAmplitude_e.ThumbProximal:
                    maximumAmplitude = 30;
                    break;
                case FingerRotationAmplitude_e.ThumbDistal:
                    maximumAmplitude = 80;
                    break;
                case FingerRotationAmplitude_e.IndexDistal:
                case FingerRotationAmplitude_e.LittleDistal:
                case FingerRotationAmplitude_e.MiddleDistal:
                case FingerRotationAmplitude_e.RingDistal:
                    maximumAmplitude = 90;
                    break;
                case FingerRotationAmplitude_e.IndexIntermediate:
                case FingerRotationAmplitude_e.MiddleIntermediate:
                case FingerRotationAmplitude_e.RingIntermediate:
                case FingerRotationAmplitude_e.LittleIntermediate:
                    maximumAmplitude = 100;
                    break;
                case FingerRotationAmplitude_e.IndexProximal:
                case FingerRotationAmplitude_e.MiddleProximal:
                case FingerRotationAmplitude_e.RingProximal:
                case FingerRotationAmplitude_e.LittleProximal:
                    maximumAmplitude = 90;
                    break;
                case FingerRotationAmplitude_e.ThumbAbduction:
                    maximumAmplitude = 0;
                    break;
            }
            return maximumAmplitude;
        }

        
    }
}