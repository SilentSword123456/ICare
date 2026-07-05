using Microsoft.Toolkit.Uwp.Notifications;

namespace ICare;

public class Notifier {
    public static void SendWarning() {
        new ToastContentBuilder()
            .AddText(AppInfo.Name)
            .AddText("You have 1 minute until your break.")
            .AddButton(new ToastButton()
                .SetContent("Skip")
                .AddArgument("action", "skip"))
            .AddButton(new ToastButton()
                .SetContent("Snooze 5 min")
                .AddArgument("action", "snooze5"))
            .AddButton(new ToastButton()
                .SetContent("Snooze 10 min")
                .AddArgument("action", "snooze10"))
            .Show();
    }
}