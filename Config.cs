using System.IO;
using System.Text.Json;

namespace ICare;

public class Config
{
    private int _workSec;
    public int WorkSec
    {
        get => _workSec;
        set { _workSec = value; Save(); }
    }
    private int _breakSec;
    public int BreakSec
    {
        get => _breakSec;
        set { _breakSec = value; Save(); }
    }
    private uint _hotkeyVk;
    public uint HotkeyVK
    {
        get => _hotkeyVk;
        set { _hotkeyVk = value; Save(); }
    }
    private int _breakStatSec;
    public int BreakStatSec
    {
        get => _breakStatSec;
        set { _breakStatSec = value; Save(); }
    }
    private string _breakMessage;
    public string BreakMessage
    {
        get => _breakMessage;
        set { _breakMessage = value; Save(); }
    }
    private int _timesSkipped;
    public int TimesSkipped
    {
        get => _timesSkipped;
        set { _timesSkipped = value; Save(); }
    }

    private static string ConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ICare",
        "config.json"
    );

    public Config()
    {
        _workSec = 20 * 60;
        _breakSec = 20;
        _hotkeyVk = 0x51;
        _breakStatSec = 0;
        _breakMessage = "Time to rest your eyes";
        _timesSkipped = 0;
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
    }

    public void Save()
    {
        var data = JsonSerializer.Serialize(this);
        File.WriteAllText(ConfigPath, data);
    }

    public void Load()
    {
        try {
            string data = File.ReadAllText(ConfigPath);

            var loaded = JsonSerializer.Deserialize<Config>(data);
            _workSec = loaded.WorkSec;
            _breakSec = loaded.BreakSec;
            _hotkeyVk = loaded.HotkeyVK;
            _breakStatSec = loaded.BreakStatSec;
            _breakMessage = loaded.BreakMessage;
            _timesSkipped = loaded.TimesSkipped;
        }
        catch (FileNotFoundException e) {
            Console.WriteLine("Config file not found, trying to generate it!");
            Save();
        }
        catch (Exception e) {
            Console.WriteLine($"Unexpected error occurred: {e.Message}");
            Console.WriteLine($"Trying to regenerate config file...");
            try {
                File.Delete(ConfigPath);
                Save();
                Console.WriteLine("Config file generated successfully!");
            }
            catch (Exception ex) {
                Console.WriteLine($"Unexpected error occurred while trying to regenerate the file. Copy the following message and report the bug on the GitHub repo:\n{ex.Message}");
            }
        }
    }
}