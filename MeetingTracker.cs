using System.Diagnostics;

namespace ICare;

public class MeetingTracker {
    private readonly string[] MeetingAppProcessNames = ["Zoom", "Teams", "ms-teams", "skype"];
    private AudioActivityDetector audioDetector;

    public MeetingTracker(AudioActivityDetector audioDetector) {
        this.audioDetector = audioDetector;
    }

    public bool IsInMeeting() {
        var running = Process.GetProcesses()
            .Select(p => p.ProcessName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var isAMeetingAppOpen = MeetingAppProcessNames.Any(name => running.Contains(name, StringComparer.OrdinalIgnoreCase));
        var isMicrophoneInUse = audioDetector.IsMicrophoneInUse();
        
        return isMicrophoneInUse && isAMeetingAppOpen;
    }
}