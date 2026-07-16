using Microsoft.Win32;

namespace ICare;

public class AutostartWatcher {
    private RegistryKey? key;
    public event Action? AppActiveChanged;
    private Task? watcher;
    private ManualResetEvent? stopEvent;
    private ManualResetEvent? registryChangedEvent;

    public void Start() {
        if (watcher != null)
            return;
        key = Registry.CurrentUser.OpenSubKey(StartupManager.ApprovedKeyPath, writable: true);
        if (key == null)
            return;
        
        registryChangedEvent = new ManualResetEvent(false);
        stopEvent = new ManualResetEvent(false);
        
        watcher = Task.Run(StartListeningForActiveChange);
        return;
    }

    public void Stop() {
        stopEvent?.Set();
        
        watcher?.Wait();
        watcher?.Dispose();
        watcher = null;
        
        
        key?.Dispose();
        key = null;
        
        registryChangedEvent?.Dispose();
        registryChangedEvent = null;
        stopEvent?.Dispose();
        stopEvent = null;
    }
    
    
    private void StartListeningForActiveChange() {
        while (true) {
            registryChangedEvent!.Reset();
            
            StartupManager.RegNotifyChangeKeyValue(
                key.Handle.DangerousGetHandle(),
                false,
                0x00000004,
                registryChangedEvent.SafeWaitHandle.DangerousGetHandle(),
                true);
            
            int signaledIndex = WaitHandle.WaitAny([registryChangedEvent, stopEvent!]);

            if (signaledIndex == 1)
                return;
            
            AppActiveChanged?.Invoke();
        }
    }
}