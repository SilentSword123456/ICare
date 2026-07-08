using System.Diagnostics;
using Microsoft.Win32;

namespace ICare;

public class SessionMonitor {
    private Timer timer;
    private bool isSessionLocked = false;
    private bool isPowerSuspended = false;

    public SessionMonitor(Timer timer) {
        this.timer = timer;
        SystemEvents.SessionSwitch += OnSessionSwitch;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    public void Stop() {
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
    } 

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e) {
        if (e.Reason == SessionSwitchReason.SessionLock) {
            Debug.WriteLine("Session lock detected");
            isSessionLocked = true;
            timer.Pause();
        }
        else if (e.Reason == SessionSwitchReason.SessionUnlock) {
            Debug.WriteLine("Session unlock detected");
            isSessionLocked = false;
            
            if (!isPowerSuspended)
                timer.Resume();
        }
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e) {
        if (e.Mode == PowerModes.Suspend) {
            Debug.WriteLine("Power mode suspended");
            isPowerSuspended = true;
            timer.Pause();
        }
        else if (e.Mode == PowerModes.Resume) {
            Debug.WriteLine("Power mode resumed");
            isPowerSuspended = false;
            if (!isSessionLocked)
                timer.Resume();
        }
    }
}