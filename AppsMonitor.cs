using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Windows.Automation;
using ICare.Models;

namespace ICare;

public class AppsMonitor {
    private Config config;
    private Process currentlyFocusedProcess;
    private Timer timer;
    public AppsFolder? currentlyActiveFolder;
    
    public AppsMonitor(Config config, Timer timer) {
        this.config = config;
        this.timer = timer;
    }

    public void Start() {
        AutomationFocusChangedEventHandler focusHandler = OnFocusChanged;
        Automation.AddAutomationFocusChangedEventHandler(focusHandler);
    }
    
    public void Stop() => Automation.RemoveAutomationFocusChangedEventHandler(OnFocusChanged);
    
    private void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e) {
        AutomationElement focusedElement = sender as AutomationElement;
        
        if (focusedElement != null) {
            int processId = focusedElement.Current.ProcessId;
            using (Process process = Process.GetProcessById(processId)) {
                Console.WriteLine("Current process is " + process.MainModule.ModuleName + " at " + process.MainModule.FileName);
                
                foreach (var folder in config.AppsFolders) {
                    Console.WriteLine("<----------------------\nFolder " + folder.Name);
                    
                    foreach (var app in folder.Apps) {
                        if (String.Compare(app.ExePath.ToLower(), process.MainModule.FileName.ToLower(), StringComparison.Ordinal) == 0) {
                            Console.WriteLine("Current app is the focused app");
                            Console.WriteLine("The rules for the current folder are:");
                            Console.WriteLine("Track time: " + folder.TrackTime);
                            Console.WriteLine("Send notification: " + folder.SendNotification);
                            Console.WriteLine("Display overlay: " + folder.ShowOverlayWarning);
                            
                            currentlyActiveFolder = folder;
                            if(folder.TrackTime == false && timer.IsPaused() == false) {
                                Console.WriteLine("Pausing timer");
                                timer.Pause();
                            }
                            else if (folder.TrackTime == true) {
                                if(timer.IsPaused() == true)
                                    timer.Resume(true);
                                timer.sendNotification = folder.SendNotification;
                                timer.displayOverlay = folder.ShowOverlayWarning;
                            }
                            
                        }
                        else {
                            currentlyActiveFolder = null;
                            timer.Resume(true);
                            timer.sendNotification = true;
                            timer.displayOverlay = true;
                        }
                        Console.WriteLine(app.Name + " at " + app.ExePath);
                    }
                }
                Console.WriteLine("---------------------->");
                
            }
        }
    }

}