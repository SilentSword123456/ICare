using System.IO;
using System.Text.Json;

namespace ICare;

public class Config
{
    public int WorkSec { get; set; }
    public int BreakSec { get; set; }
    public string Hotkey { get; set; }

    private const string ConfigPath = "config.json";

    public Config()
    {
        WorkSec = 1 * 60;
        BreakSec = 20;
        Hotkey = "Shift+Control+Q";
    }

    public void Save()
    {
        var data = JsonSerializer.Serialize(this);
        File.WriteAllText(ConfigPath, data);
    }

    public void Load()
    {
        try
        {
            string data = File.ReadAllText(ConfigPath);

            var loaded = JsonSerializer.Deserialize<Config>(data);
            WorkSec = loaded.WorkSec;
            BreakSec = loaded.BreakSec;
            Hotkey = loaded.Hotkey;
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("Config file not found, trying to generate it!");
            Save();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error occurred: {e.Message}");
            Console.WriteLine($"Trying to regenerate config file...");
            try
            {
                File.Delete(ConfigPath);
                Save();
                Console.WriteLine("Config file generated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error occurred while trying to regenerate the file. Copy the following message and report the bug on the GitHub repo:\n{ex.Message}");
            }
        }
    }
}