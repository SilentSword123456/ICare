using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace ICare;

public class AudioActivityDetector {
    private MMDeviceEnumerator enumerator;
    private MMDevice device;

    public AudioActivityDetector() {
        enumerator = new MMDeviceEnumerator();
        device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
    }
    
    public bool IsAudioPlaying() {
        var sessions = device.AudioSessionManager.Sessions;

        for (int i = 0; i < sessions.Count; i++) {
            var session = sessions[i];
            if (session.State == AudioSessionState.AudioSessionStateActive && session.AudioMeterInformation.MasterPeakValue > 0.0001f)
                return true;
        }
        return false;
    }
}