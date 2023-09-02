namespace ContactGloveSDK
{
    public interface IControllerInput
    {
        public bool GetControllerInput(HandSides hand, ControllerBoolInputType type);

        public float GetControllerInput(HandSides hand, ControllerFloatInputType type);

        public void AddOnControllerInputHandler(HandSides hand, ControllerBoolInputType type, ControllerInputHandler handler);
        
        public void AddOffControllerInputHandler(HandSides hand, ControllerBoolInputType type, ControllerInputHandler handler);
    }
}