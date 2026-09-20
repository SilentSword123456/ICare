<div align="center">

# ICare

**A break reminder for Windows that actually understands what you're doing before it interrupts you.**

<img src="https://github.com/user-attachments/assets/a29ff233-0f16-49d1-9b00-67bde6d389f4" width="600" alt="ICare dashboard" />

[Download the installer](https://github.com/SilentSword123456/ICare/releases/latest/download/ICare-win-Setup.exe) ·
[Portable .exe](https://github.com/SilentSword123456/ICare/releases/latest/download/ICare.exe) ·
[Report a bug](https://github.com/SilentSword123456/ICare/issues)

![Hackatime Badge](https://hackatime.hackclub.com/api/v1/badge/U0A531VD30C/SilentSword123456/ICare)

*Windows 10/11 only (x64).*

</div>

---

## Why I built this

My doctor told me to start looking away from the screen every 20 minutes or my eyes were going to hate me XD. So I built the tool
that would force me to do it. It worked well enough that I kept adding to it, and now it's a full-blown Windows app
with a dashboard, theming, smart pausing, and enough edge-case handling that it doesn't interrupt you mid-boss-fight
or mid-presentation.

It's tiny, it's fast, and it runs quietly in your tray until it's time to look away.

## What it looks like

<img width="854" height="480" alt="ICare" src="https://github.com/user-attachments/assets/a92c4483-baf9-4110-95af-4c198dbf76bc" />
<img src="https://github.com/user-attachments/assets/908a939e-8213-456c-bd54-f5040ae5e4ee" width="600" alt="Break screen" />

## Features

### How the break works

When your work timer runs out, every monitor displays a full-screen overlay. The primary screen shows a countdown and
a message; the others just show a calm background image that matches your theme (so it dosent flash you if its night-time). Your keyboard is
locked for the duration (Ctrl+Alt+Del still works, in case something goes wrong), so you can't just tab back into
what you were doing. If you genuinely need to skip it, a customizable hotkey (`Ctrl+Shift+<key>`, default `Q`) lets
you skip the upcoming break, but the countdown keeps running in the background regardless.

A minute before a break, you get a Windows notification with **Skip**, **Snooze 5 min**, and **Snooze 10 min**
buttons, so you're never caught off guard.

### When the app knows it shouldn't interrupt you

This is the part I spent the most time on, because if it would fire during a boss fight or a client call you would want to uninstall it IMEDIATTLY.

- **Meetings** — if a meeting app (Zoom, Teams, Skype) is running *and* your mic is actually in use, ICare swaps the
  hard blackout for a small on-screen countdown warning instead, so you're not muted and locked out mid-sentence.
- **Fullscreen apps** — games, movies, presentations. ICare detects real fullscreen windows (not just maximized ones)
  and gives you the same soft warning instead of yanking you out instantly.
- **Idle time** — step away from your desk and the work timer pauses itself after two minutes of no input, then picks
  up right where it left off when you're back, unless you're playing audio (watching something, listening to
  music), in which case it assumes you're still around.
- **Lock screen, sleep, and display-off** — all pause the timer automatically. No more coming back to a "break" that
  fired while your laptop was in a bag.

### App Folders [TEMPORARY DISABLED]

Beyond the automatic cases above, you can group specific apps into folders and decide exactly how ICare should treat
each group while they're focused: keep tracking the time normally, pause entirely, or count down but skip
notifications/warnings for that folder. It's useful for anything you don't want to rely on ICare's automatic detection systems for, like a specific game, a video call app you use that isn't auto-detected, whatever.

### Dashboard

A small dashboard gives you a live circular countdown to the next break, how many minutes of break time you've
actually taken, and how many breaks you've skipped (so you can't lie to yourself about it). It opens from the system
tray and closing it just hides it. ICare keeps running until you quit it from the tray icon.

### Settings

Work/break duration, the skip hotkey, and light/dark theme (it follows your Windows theme by default, or you can
override it) all update live with no save button — everything's written to a config file in `%AppData%` the instant
you change it. There's also a one-click reset to defaults, and launching ICare a second time just brings the existing
dashboard forward instead of spawning a duplicate instance.

### It stays out of your way!

- Optional auto-start with Windows, with a warning if Windows silently disabled it for you (if it is disabled by windows, just go into start-up apps from task manager and enable it back on. Also, I must confess, I'm a little proud of this check q(≧▽≦q)).
- Self-updating: ICare checks GitHub Releases for new versions and can download and install them in place via
  [Velopack](https://velopack.io), or you can use the portable build and skip auto-updates entirely.
- The portable version runs as a single self-contained `.exe`. It dosent need any external dependecies (like .NET for example).

## How it works, in short

ICare starts a background process that survives closing the dashboard window. It tracks elapsed time against your
configured work duration, checks the exceptions (meeting / fullscreen / paused app folder / idle) as the break
approaches, and either shows a break or a warning accordingly. All of the pause/resume/skip/snooze logic funnels
through a single timer that's aware of all of these signals at once, so they compose correctly. Being idle during a
meeting behaves sensibly, snoozing during a fullscreen warning behaves sensibly, and so on.

## Privacy

ICare uses [Sentry](https://sentry.io) for crash reporting. If the app crashes or an update check fails, it sends a
report with the stack trace, the app version, and basic OS info. That's it, no usage tracking, no telemetry on how
you use the app day to day. There's currently no in-app toggle to disable this; if that matters to you, build from
source with the Sentry `Init` call in `App.xaml.cs` removed.

## Installation

Grab [the installer](https://github.com/SilentSword123456/ICare/releases/latest/download/ICare-win-Setup.exe) if you
want auto-updates and Start Menu integration, or the [portable .exe](https://github.com/SilentSword123456/ICare/releases/latest/download/ICare.exe)
if you'd rather run it like so. Neither needs admin privileges. On first launch the
dashboard opens automatically. You can click the tray icon to bring it back, or right-click it to quit.

Windows 10/11 (x64) only — ICare leans on WPF and Win32 APIs (`user32`, `dwmapi`, `shcore`) for the fullscreen, idle,
and hotkey detection, so there's no Mac or Linux build.

## Building from source

```bash
git clone https://github.com/SilentSword123456/ICare.git
cd ICare
dotnet build
```

Requires the .NET 10 SDK with Windows targeting. `ICare.csproj` is the app; `ICare Tests/` has the NUnit test suite
(config persistence, app monitoring, fullscreen detection, view logic). Releases are built and signed automatically
by GitHub Actions on any `v*` tag and packaged with Velopack for delta updates.

## Contributing

Issues and PRs are welcome. If you want the Ctrl+Shift skip-hotkey prefix to be configurable too, or have another
edge case ICare should know to ignore, open an issue, PLEASE. That's exactly the kind of thing this project is for, to improve edge case after edge case until it can handle every situation.
