                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
namespace ContactGloveSDK
{
    internal struct DataLength
    {
        // 4byte(float) 3 values per finger, except thumb
        internal const int FingerRot = 5 * 3 + 1;
        internal const int FingerRotBytes = 4 * 3 * 5 + 4;
        
        internal const int FingerBone = 3 * 5;
        internal const int FingerHandPhysics = 4 ;
    
        // 4byte(float) * 10 rotation data
        internal const int FlexData = 4 * 10;
    
        internal const int ControllerInput = 39;
        
        internal const int EachHandHapticsDataBytes = 3;
        internal const int HapticsDataBytes = EachHandHapticsDataBytes * 2;
    
        internal const int HandLength = 2;
        
        internal const int CollisionNum = 32;

        internal const int BackHapticsTypeNum = 16;
        internal const int MaxFingerHapticsStength = 16;

        internal const int VibrationLength = 4;
    }
}

