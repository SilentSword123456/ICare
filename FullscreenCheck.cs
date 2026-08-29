using System.Runtime.InteropServices;

namespace ICare;

public class FullscreenCheck {
    
    public enum UserNotificationState {
        QUNS_NOT_PRESENT = 1,
        QUNS_BUSY = 2,
        QUNS_RUNNING_D3D_FULL_SCREEN = 3,
        QUNS_PRESENTATION_MODE = 4,
        QUNS_ACCEPTS_NOTIFICATIONS = 5,
        QUNS_QUIET_TIME = 6
    };

    [DllImport("shell32.dll")]
    static extern int SHQueryUserNotificationState(out UserNotificationState userNotificationState);

    public static bool IsAnAppFullscreen() { //TODO make the function also return true if a fullscreen window is opened in another window besides the main one
        UserNotificationState state;
        var returnVal = SHQueryUserNotificationState(out state);

        return state == UserNotificationState.QUNS_BUSY || state == UserNotificationState.QUNS_RUNNING_D3D_FULL_SCREEN;
    }
}