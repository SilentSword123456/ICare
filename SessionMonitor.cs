using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;

namespace ICare;

public class SessionMonitor {
    private Timer timer;
    private bool isSessionLocked = false;
    private bool isPowerSuspended = false;
    private bool isDisplayOff = false;
    private DispatcherTimer dispatcher;
    private AudioActivityDetector audioDetector;
    
    [StructLayout(LayoutKind.Sequential)]
    private struct POWERBROADCAST_SETTING {
        public Guid PowerSetting;
        public uint DataLength;
        public byte Data;
    }
    
    private static readonly Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);


    public SessionMonitor(Window window, Timer timer, AudioActivityDetector audioDetector) {
        ArgumentNullException.ThrowIfNull(window);
        
        this.audioDetector = audioDetector;
        this.timer = timer;
        SystemEvents.SessionSwitch += OnSessionSwitch;
        SystemEvents.PowerModeChanged += OnPowerModeChanged;

        var source = PresentationSource.FromVisual(window) as HwndSource;
        source!.AddHook(handleHook);

        var guid = GUID_CONSOLE_DISPLAY_STATE;
        RegisterPowerSettingNotification(new WindowInteropHelper(window).Handle, ref guid, 0);
        
        dispatcher = new DispatcherTimer();
        dispatcher.Interval = TimeSpan.FromSeconds(1);
        dispatcher.Tick += ManageLastAction;
        dispatcher.Start();
    }

    private void ManageLastAction(object? sender, EventArgs eventArgs) {
        var lastActionTime = IdleTimeDetector.GetIdleTime();
        
        bool isAudioPlaying = audioDetector.IsAudioPlaying();
        
        if (lastActionTime.TotalSeconds >= 120 && !isAudioPlaying && !timer.IsPaused()
            && !isPowerSuspended && !isDisplayOff && !isSessionLocked) {
            timer.Pause();
            timer.AddTimeBack(lastActionTime);
        }
        else if (lastActionTime.TotalSeconds < 120  && timer.IsPaused() && !isPowerSuspended && !isDisplayOff && !isSessionLocked) {
            timer.Resume();
        }
    }

    public void Stop() {
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        dispatcher.Stop();
    } 

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e) {
        if (e.Reason == SessionSwitchReason.SessionLock) {
            isSessionLocked = true;
            timer.Pause();
        }
        else if (e.Reason == SessionSwitchReason.SessionUnlock) {
            isSessionLocked = false;
            
            if (!isPowerSuspended && !isDisplayOff)
                timer.Resume();
        }
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e) {
        if (e.Mode == PowerModes.Suspend) {
            isPowerSuspended = true;
            timer.Pause();
        }
        else if (e.Mode == PowerModes.Resume) {
            isPowerSuspended = false;
            if (!isSessionLocked && !isDisplayOff)
                timer.Resume();
        }
    }
    
    private IntPtr handleHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
        if (msg == 536 && wParam.ToInt32() == 0x8013) { //536 is equal to WM_POWERBROADCAST according to Microsoft's doc, the other one I hope is right
            var setting = Marshal.PtrToStructure<POWERBROADCAST_SETTING>(lParam);
            
            if (setting.PowerSetting != GUID_CONSOLE_DISPLAY_STATE)
                return IntPtr.Zero;
            
            if (setting.Data == 0) {
                isDisplayOff = true;
                timer.Pause();
            }
            else if (setting.Data == 1) {
                isDisplayOff =  false;
                if (!isPowerSuspended && !isSessionLocked)
                    timer.Resume();
            }
        }
        return IntPtr.Zero;
    }
}