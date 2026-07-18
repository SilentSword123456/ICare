using System.Diagnostics;
using ICare.Views;

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
    private TimeSpan workTimeModifier;
    private BreakOverlayWindow breakOverlayWarning;
    private MeetingTracker meetingTracker;
    
    public TimeSpan Remaining => currentWorkTime - (workStopwatch.Elapsed - workTimeModifier) > TimeSpan.Zero
        ? currentWorkTime - (workStopwatch.Elapsed - workTimeModifier)
        : TimeSpan.Zero;

    public Timer(Config config, Keyboard keyboard, MeetingTracker meetingTracker) {
        this.config = config;
        this.keyboard = keyboard;
        this.meetingTracker = meetingTracker;
        
        globalCts = new CancellationTokenSource();
        workStopwatch = new();
        breakStopwatch = new();
        breakOverlayWarning = new BreakOverlayWindow(config);
    }

    public async Task Start() {
        var globalToken = globalCts.Token;

        while (!globalToken.IsCancellationRequested) {
            currentWorkTime = TimeSpan.FromSeconds(config.WorkSec);
            SkipNext = false;
            isPaused = false;
            workTimeModifier = TimeSpan.Zero;
            var isInMeeting = meetingTracker.IsInMeeting();
            workStopwatch.Restart();

            try {
                while (true) {
                    cycleCts = CancellationTokenSource.CreateLinkedTokenSource(globalToken);
                    
                    var remaining = Remaining;

                    TimeSpan waitTime;
                    if (isPaused)
                        waitTime = Timeout.InfiniteTimeSpan;
                    else if (remaining > TimeSpan.FromSeconds(60))
                        waitTime = remaining - TimeSpan.FromSeconds(60);
                    else 
                        waitTime = isInMeeting && remaining > TimeSpan.FromSeconds(10) ? remaining - TimeSpan.FromSeconds(10) : remaining;


                    try {
                        Console.WriteLine("Waiting for " + waitTime.TotalSeconds + " seconds");
                        await Task.Delay(waitTime, cycleCts.Token);
                    }
                    catch (TaskCanceledException) {
                        if (globalToken.IsCancellationRequested)
                            throw;
                        continue;
                    }

                    isInMeeting = meetingTracker.IsInMeeting();

                    if (isPaused)
                        continue;
                    
                    remaining = Remaining;

                    if (remaining <= TimeSpan.FromSeconds(60) && remaining != TimeSpan.Zero && !(isInMeeting && remaining <= TimeSpan.FromSeconds(10)) && !SkipNext) {
                        Console.WriteLine("Sending  break notification at " + remaining.TotalSeconds + " seconds left");
                        Notifier.SendWarning();
                        continue;
                    }
                    
                    if (!SkipNext && isInMeeting && remaining >= TimeSpan.FromSeconds(1) && remaining <= TimeSpan.FromSeconds(10)) {
                        Console.WriteLine("Starting break warning with " + remaining.TotalSeconds + " seconds left");
                        await breakOverlayWarning.StartBreakWarning(TimeSpan.FromSeconds(10), globalToken);
                        continue;
                    }

                    if (SkipNext && Remaining != TimeSpan.Zero)
                        continue;

                    break;
                }
                
                Console.WriteLine("Exited timer loop, checking if we should trigger the break");
                if(!SkipNext) {
                    Console.WriteLine("Triggering break");
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
        globalCts.Dispose();
        globalCts = new CancellationTokenSource();
        _ = Start();
    }

    public void SkipNextBreak() {
        if (SkipNext) return;
        SkipNext = true;
        config.TimesSkipped++;
        breakOverlayWarning.StopBreakWarning();
    }

    public void Stop() => globalCts.Cancel();

    public void SnoozeBreak(TimeSpan time) {
        currentWorkTime += time;
        cycleCts?.Cancel();
    }
    
    public void AddTimeBack(TimeSpan time) {
        workTimeModifier += time;
        cycleCts?.Cancel();
    }
    
    public bool IsPaused() => isPaused;
}