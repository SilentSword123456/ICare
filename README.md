# ICare
ICare is an app I build to remind (or force ^_^) me to take a break from the screen every 20min because my doctor said so. I figured this would help other people, so I just started building it. It has a nice UI, it is VERY lightweight, and it does its job.

# Instalation
Head over to https://github.com/SilentSword123456/ICare/releases/latest. Download it, and then run it (it doesn't need Admin privileges). You should be able to access the dashboard via a system-tray icon by clicking it, and quit the app by right-clicking on the icon and pressing quit.

# How it works
When launched, ICare automatically starts a process in the system tray to keep it alive. Even if ++++++you close the dashboard, ICare will still run. To close it you can right-click on the icon from the system-tray and click "quit".
The app works by having a timer that once it ends it triggers the break. During the break the keyboard will be disabled (You can use ctrl+alt+delete to exit if something goes wrong) and a countdown with a break message will appear on the main screen, while the other screens should display just a black window.
If you are in a game or doing something very important and you cant use the mouse to dismiss the timer from the notification you can use the shortcut ctrl+shift+Q by defaults to entirely skip the next break. The timer will continue, but when the break should be triggerd, it wil just skip it. You can customize the hotkey by choosing any other one from A to Z, but the ctrl+shift "prefix" cant be changed (for now, if you would like to see this future added just fire up a short issue).
ICare uses a config file located in AppData to store its configuration, and don't worry, it automatically saves on any change when editing the settings, you don't need to press anything.

