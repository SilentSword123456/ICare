using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace ICare;

public class AudioActivityDetector {
    private MMDeviceEnumerator enumerator;
    private MMDevice renderDevice;
    private MMDevice captureDevice;

    public AudioActivityDetector() {
        enumerator = new MMDeviceEnumerator();
        renderDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        captureDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
    }
    
    public bool IsAudioPlaying() => IsDeviceActive(renderDevice);
    

    public bool IsMicrophoneInUse() => IsDeviceActive(captureDevice);

    private bool IsDeviceActive(MMDevice device) {
        var sessions = device.AudioSessionManager.Sessions;

        for (int i = 0; i < sessions.Count; i++) {
            var session = sessions[i];
            if (session.State == AudioSessionState.AudioSessionStateActive && session.AudioMeterInformation.MasterPeakValue > 0.0001f)
                return true;
        }
        return false;
    }
}