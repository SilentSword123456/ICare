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
                currentlyActiveFolder = null;
                //Console.WriteLine("Current process is " + process.ProcessName + " at " + process.MainModule.FileName);
                
                foreach (var folder in config.AppsFolders) {
                    //Console.WriteLine("<----------------------\nFolder " + folder.Name);
                    
                    foreach (var app in folder.Apps) {
                        if (String.Compare(app.ExePath, process.MainModule.FileName, StringComparison.Ordinal) == 0
                            || string.Equals(app.Name, process.ProcessName, StringComparison.CurrentCultureIgnoreCase)) {
                            //Console.WriteLine("Current app is the focused app");
                            
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
                        
                        
                        //Console.WriteLine(app.Name + " at " + app.ExePath);
                    }
                }
                //Console.WriteLine("---------------------->");
                
            }
            
            if (currentlyActiveFolder == null) {
                timer.Resume(true);
                timer.sendNotification = true;
                timer.displayOverlay = false;
            }
        }
    }

}