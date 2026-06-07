namespace ICare;

public class Timer
{
    private readonly Config config;
    public bool SkipNext { get; set; } = false;
    public DateTime BreakDate { get; private set; }
    private CancellationTokenSource cts;
    private Action triggerBreak;

    public Timer(Config _config, Action _triggerBreak) {
        config = _config;
        triggerBreak = _triggerBreak;
        cts = new CancellationTokenSource();
    }

    public async Task Start() {
        var token = cts.Token;
        while (!token.IsCancellationRequested) {
            BreakDate = DateTime.Now.AddSeconds(config.WorkSec);
            SkipNext = false;
            try {
                await Task.Delay(config.WorkSec * 1000 - 60 * 1000, token);
                Notifier.SendWarning();
                await Task.Delay(60 * 1000, token);
                if (!SkipNext)
                    triggerBreak();
            }
            catch (TaskCanceledException) {
                break;
            }
        }
    }

    public void Restart() {
        cts.Cancel();
        cts = new CancellationTokenSource();
        _ = Start();
    }

    public void Stop() {
        cts.Cancel();
    }
}