# Description
<img width="3840" height="2880" alt="screenshot-studio-1783800517833" src="https://github.com/user-attachments/assets/a29ff233-0f16-49d1-9b00-67bde6d389f4" />




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

![Hackatime Badge](https://hackatime.hackclub.com/api/v1/badge/U0A531VD30C/SilentSword123456/ICare)

