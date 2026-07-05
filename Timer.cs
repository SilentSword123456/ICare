using System.Diagnostics;

namespace ICare;

public class Timer {
    private readonly Config config;
    private Keyboard keyboard;
    public bool SkipNext { get; set; } = false;
    public TimeSpan currentWorkTime;
    private CancellationTokenSource globalCts;
    private CancellationTokenSource cycleCts;
    private readonly Stopwatch workStopwatch;
    private readonly Stopwatch breakStopwatch;
    private Boolean isPaused;

    public TimeSpan Remaining => currentWorkTime - workStopwatch.Elapsed > TimeSpan.Zero
        ? currentWorkTime - workStopwatch.Elapsed
        : TimeSpan.Zero;

    public Timer(Config config, Keyboard keyboard) {
        this.config = config;
        this.keyboard = keyboard;
        globalCts = new CancellationTokenSource();
        workStopwatch = new();
        breakStopwatch = new();
    }

    public async Task Start() {
        var globalToken = globalCts.Token;

        while (!globalToken.IsCancellationRequested) {
            currentWorkTime = TimeSpan.FromSeconds(config.WorkSec);
            SkipNext = false;
            isPaused = false;
            workStopwatch.Restart();

            try {
                while (true) {
                    cycleCts = CancellationTokenSource.CreateLinkedTokenSource(globalToken);

                    TimeSpan waitTime;
                    if (isPaused)
                        waitTime = Timeout.InfiniteTimeSpan;
                    else if (Remaining > TimeSpan.FromSeconds(60))
                        waitTime = Remaining - TimeSpan.FromSeconds(60);
                    else
                        waitTime = Remaining;


                    try {
                        await Task.Delay(waitTime, cycleCts.Token);
                    }
                    catch (TaskCanceledException) {
                        if (globalToken.IsCancellationRequested)
                            throw;
                        continue;
                    }

                    if (isPaused)
                        continue;

                    if (Remaining <= TimeSpan.FromSeconds(60) && Remaining != TimeSpan.Zero && !SkipNext) {
                        Notifier.SendWarning();
                        continue;
                    }

                    break;
                }

                if (!SkipNext) {
                    await BlackoutWindow.TriggerBreak(keyboard, config);
                }
            }
            catch (TaskCanceledException) {
                break;
            }
        }
    }

    public void Pause() {
        if (isPaused) return;
        isPaused = true;
        workStopwatch.Stop();
        cycleCts?.Cancel();
        breakStopwatch.Restart();
    }

    public void Resume() {
        if (!isPaused) return;
        isPaused = false;
        workStopwatch.Start();
        cycleCts?.Cancel();
        if (breakStopwatch.Elapsed >= TimeSpan.FromSeconds(config.BreakSec)) {
            breakStopwatch.Stop();
            Restart();
        }
    }

    public void Restart() {
        globalCts.Cancel();
        globalCts = new CancellationTokenSource();
        _ = Start();
    }

    public void SkipNextBreak() {
        if (SkipNext) return;
        SkipNext = true;
        config.TimesSkipped++;
    }

    public void Stop() => globalCts.Cancel();

    public void SnoozeBreak(int minutes) {
        currentWorkTime += TimeSpan.FromMinutes(minutes);
        cycleCts?.Cancel();
    }
}