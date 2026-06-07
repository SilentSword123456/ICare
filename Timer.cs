namespace ICare;

public class Timer
{
    private readonly Config _config;
    public bool SkipNext { get; set; } = false;
    public DateTime BreakDate { get; private set; }

    public Timer(Config config) {
        _config = config;
    }

    public async Task Start(CancellationToken token, Action triggerBreak) {
        while (!token.IsCancellationRequested) {
            BreakDate = DateTime.Now.AddSeconds(_config.WorkSec);
            SkipNext = false;
            try {
                await Task.Delay(_config.WorkSec * 1000 - 60 * 1000, token);
                //TODO add notification call send
                await Task.Delay(60 * 1000, token);
                if (!SkipNext)
                    triggerBreak();
            }
            catch (TaskCanceledException) {
                break;
            }
        }
        
        Console.WriteLine("Timer stopped");
        
    }
}