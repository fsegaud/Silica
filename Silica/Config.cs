namespace Silica;

public static class Config
{
    public static string HomePageName => _userConfig.UserHomePageName;
    public static string Css => _userConfig.UserCss;
    public static string[] ExcludedFiles => _userConfig.UserExcludedFiles;
    
    private static UserConfig _userConfig = null!;
    
    public static bool CreateFileIfNotExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            return false;
        }

        _userConfig = new UserConfig();
            
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(_userConfig, Newtonsoft.Json.Formatting.Indented);
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
        _userConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<UserConfig>(json)!;
        
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

    private class UserConfig
    {
        public readonly string UserHomePageName = "Silica";
        public readonly string UserCss = "light.css";
        public readonly string[] UserExcludedFiles =
            [".obsidian", ".silica", ".git", ".gitignore", ".gitattributes", "README.md", "LICENSE.md", "CHANGELOG.md"];
    }
}
