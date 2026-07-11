# Description
<img width="666" height="443" alt="image" src="https://github.com/user-attachments/assets/b0303323-21d5-46a2-88de-95a8f6e9b2e2" />


ICare is an app I build to remind (or force ^_^) me to take a break from the screen every 20min because my doctor said
so. I figured this would help other people, so I just started building it. It has a nice UI, it is VERY lightweight, and
it does its job.

# Instalation

Head over to https://github.com/SilentSword123456/ICare/releases/latest. Download it, and then run it (it doesn't need
Admin privileges). You can access the dashboard via the system-tray icon by just clicking on it and stop the app by
right-clicking the icon and pressing quit.

# How it works

When launched, ICare automatically starts a process in the system tray to keep it alive. Even if you close the
dashboard, ICare is still gonna run.

The app works by keeping track of the time spent at the computer and triggering the break once the configured work time is reached. During the break the keyboard will be
disabled (You can use ctrl+alt+delete to exit if something goes wrong) and a countdown with a break message will appear
on the main screen, while the other screens should display just a nice photo.

If you are in a game or doing something very important, and you can't use the mouse to dismiss the timer from the
notification, you can use the shortcut ``ctrl+shift+Q`` (by default) to entirely skip the next break. The timer will continue,
but once it ends it will just continue without starting the break. You can customize the hotkey by choosing any from
A to Z, but the ctrl+shift "prefix" cant be changed (for now, if you would like to see this future added just write a
short issue).

ICare uses a config file located in AppData to store its configuration, and don't worry, it automatically saves on any
change when editing the settings, you don't need to press anything.

