using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using ICare.Utilities;

namespace ICare;

public class Config {
    private int _workSec;

    public int WorkSec {
        get => _workSec;
        set {
            if (value < 0)
                return;
            _workSec = value;
            
            if(Autosave)
                Save();
        }
    }

    private int _breakSec;

    public int BreakSec {
        get => _breakSec;
        set {
            if (value < 0)
                return;
            _breakSec = value;
            
            if(Autosave)
                Save();
        }
    }

    private uint _hotkeyVk;

    public uint HotkeyVk {
        get => _hotkeyVk;
        set {
            _hotkeyVk = value;
            
            if(Autosave)
                Save();
        }
    }

    private int _breakStatSec;

    public int BreakStatSec {
        get => _breakStatSec;
        set {
            _breakStatSec = value;
            
            if(Autosave)
                Save();
        }
    }

    private string _breakMessage;

    public string BreakMessage {
        get => _breakMessage;
        set {
            _breakMessage = value;
            
            if(Autosave)
                Save();
        }
    }

    private int _timesSkipped;

    public int TimesSkipped {
        get => _timesSkipped;
        set {
            _timesSkipped = value;
            
            if(Autosave)
                Save();
        }
    }

    private Theme _theme;
    public Theme Theme {
        get => _theme;
        set {
            _theme = value;
            
            if(Autosave)
                Save();
        }
    }

    private ObservableCollection<AppsFolder> _appsFolders;
    public ObservableCollection<AppsFolder> AppsFolders {
        get => _appsFolders;
        set  {
            _appsFolders = value;
            
            if(Autosave)
                Save();
        }
    }

    public bool Autosave = true;

    private static string ConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        AppInfo.Name,
        "config.json"
    );

    public Config() {
        _workSec = 20 * 60;
        _breakSec = 20;
        _hotkeyVk = 0x51;
        _breakStatSec = 0;
        _breakMessage = "Time to rest your eyes";
        _timesSkipped = 0;
        _theme = AppInfo.SystemTheme;
        _appsFolders = new ObservableCollection<AppsFolder>();
        _appsFolders.CollectionChanged += (s, e) => {
            if (e.NewItems != null)
                foreach (AppsFolder folder in e.NewItems)
                    SubscribeFolder(folder);
            if (Autosave) 
                Save();
        };
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
    }

    private void SubscribeFolder(AppsFolder folder) {
        folder.PropertyChanged += (s, e) => { if (Autosave) Save(); };
        folder.Apps.CollectionChanged += (s, e) => { if (Autosave) Save(); };
    }

    public void ResetToDefault() {
        WorkSec = 20 * 60;
        BreakSec = 20;
        HotkeyVk = 0x51;
        BreakStatSec = 0;
        BreakMessage = "Time to rest your eyes";
        TimesSkipped = 0;
        _appsFolders = new ObservableCollection<AppsFolder>();
        _appsFolders.CollectionChanged += (s, e) => {
            if (e.NewItems != null)
                foreach (AppsFolder folder in e.NewItems)
                    SubscribeFolder(folder);
            if (Autosave) 
                Save();
        };
        Theme = AppInfo.SystemTheme;
    }

    public void Save() {
        var data = JsonSerializer.Serialize(this);
        File.WriteAllText(ConfigPath, data);
    }

    public void Load() {
        try {
            string data = File.ReadAllText(ConfigPath);

            var loaded = JsonSerializer.Deserialize<Config>(data);
            _workSec = loaded.WorkSec;
            _breakSec = loaded.BreakSec;
            _hotkeyVk = loaded.HotkeyVk;
            _breakStatSec = loaded.BreakStatSec;
            _breakMessage = loaded.BreakMessage;
            _timesSkipped = loaded.TimesSkipped;
            _theme = loaded.Theme;
            _appsFolders = loaded.AppsFolders;
            foreach (var folder in _appsFolders) {
                foreach (var app in folder.Apps)
                    app.Icon = IconHelper.GetIconForExe(app.ExePath);
                SubscribeFolder(folder);
            }
        }
        catch (FileNotFoundException e) {
            Console.WriteLine("Config file not found, trying to generate it!");
            Save();
        }
        catch (Exception e) {
            Console.WriteLine($"Unexpected error occurred: {e.Message}");
            Console.WriteLine("Config file appears corrupted; leaving it in place for inspection and using in-memory defaults for this session.");
        }
    }
}