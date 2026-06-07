using Microsoft.Toolkit.Uwp.Notifications;

namespace ICare;

public class Notifier
{
    public static void SendWarning()
    {
        new ToastContentBuilder()
            .AddText("ICare")
            .AddText("You have 1 minute before your break starts.")
            .AddButton(new ToastButton()
                .SetContent("Skip")
                .AddArgument("action", "skip"))
            .Show();
    }
}