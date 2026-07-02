namespace ICare;

public class Timer
{
    private readonly Config config;
    public bool SkipNext { get; set; } = false;
    public DateTime BreakDate { get; private set; }
    public TimeSpan currentWorkTime;
    private CancellationTokenSource cts;
    private Func<Task> triggerBreak;

    public Timer(Config _config, Func<Task> _triggerBreak) {
        config = _config;
        triggerBreak = _triggerBreak;
        cts = new CancellationTokenSource();
    }

    public async Task Start() {
        var token = cts.Token;
        while (!token.IsCancellationRequested) {
            currentWorkTime = TimeSpan.FromSeconds(config.WorkSec);
            BreakDate = DateTime.Now + currentWorkTime;
            SkipNext = false;
            try
            {
                while (DateTime.Now < BreakDate && !token.IsCancellationRequested)
                {
                    var SecondsLeft = (int)(BreakDate - DateTime.Now).TotalSeconds;
                    if (SecondsLeft > 60)
                        await Task.Delay(SecondsLeft * 1000 - 60 * 1000, token);
                    Notifier.SendWarning();
                    await Task.Delay(SecondsLeft * 1000, token);
                }

                if (!SkipNext)
                    await triggerBreak();
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    public void Restart() {
        cts.Cancel();
        cts = new CancellationTokenSource();
        _ = Start();
    }
    
    public void SkipNextBreak()
    {
        if (SkipNext == true)
            return;
        SkipNext = true;
        config.TimesSkipped++;
    }

    public void Stop() {
        cts.Cancel();
    }

    public void SnoozeBreak(int minutes) {
        BreakDate = BreakDate.AddMinutes(minutes);
        currentWorkTime = currentWorkTime + TimeSpan.FromMinutes(minutes);
    }
}