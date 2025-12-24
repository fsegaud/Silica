namespace Silica;

public static class Config
{
    public static ExportConfig Export = null!;
    
    public static bool CreateFileIfNotExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            return false;
        }

        Export = new ExportConfig();
            
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(Export, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(filePath, json);

        return true;
    }
    
    public static bool LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }
        
        string json = File.ReadAllText(filePath);
        Export = Newtonsoft.Json.JsonConvert.DeserializeObject<ExportConfig>(json)!;
        
        return true;
    }

    private static string _configBackup = string.Empty;

    public static bool TryBackupConfigFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            _configBackup = File.ReadAllText(filePath);
            return true;
        }

        return false;
    }

    public static bool TryRestoreConfigFile(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(_configBackup))
        {
            File.WriteAllText(filePath, _configBackup);
            _configBackup = string.Empty;
            return true;
        }

        return false;
    }

    public class ExportConfig
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Global
        // ReSharper disable ConvertToConstant.Global
        public string HomePageName = "Silica";
        public string CssStyle = "light.css";
        public bool CssEmbedded = false;
        public bool ShortUrl = false;
        public string[] ExcludedFiles =
            [".obsidian", ".silica", ".git", ".gitignore", "README.md"];
        // ReSharper restore FieldCanBeMadeReadOnly.Global
        // ReSharper restore ConvertToConstant.Global

        public override string ToString()
        {
            string result = "Current configuration:";
            result += $"\n    - HomePageName: [cyan]{HomePageName}[/]";
            result += $"\n    - CssStyle: [cyan]{CssStyle}[/]";
            result += $"\n    - CssEmbedded: [cyan]{CssEmbedded}[/]";
            result += $"\n    - ShortUrl: [cyan]{ShortUrl}[/]";
            result += $"\n    - ExcludedFiles: [cyan]{string.Join(", ", ExcludedFiles)}[/]";
            return result;
        }
    }
}
