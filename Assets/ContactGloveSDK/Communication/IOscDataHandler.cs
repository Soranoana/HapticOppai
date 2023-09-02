using uOSC;

namespace ContactGloveSDK
{
    internal interface IOscDataHandler
    {
        string GetOscIpAddress();

        void ClosePairingServer();

        /// <summary>
        /// Check if the data is updated and update the data on each OscModes
        /// </summary>
        /// <param name="mode">send mode defined in enum OscModes</param>
        /// <param name="newData">send data in byte[]</param>
        /// <returns></returns>
        bool IsDataUpdated(OscModes mode, byte[] newData);

        void OnDataReceived(Message message);

        IControllerInput GetControllerInput();
    }
}