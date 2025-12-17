using Silica;

// Get parameters.
if (args.Length == 0)
{
    Console.Error.WriteLine("Use : Silica.exe <ObsidianProjectPath> [-v]");
    return -1;
}

Params.ObsidianProjectPath = args[0];
if (!Path.Exists(Params.ObsidianProjectPath))
{
    Console.Error.WriteLine("Error: Could not find \"{0}\"", Params.ObsidianProjectPath);
    return -1;
}

Params.Verbose = args.Contains("-v");

// Print parameters
ConsoleHelper.AppLogo(); 
ConsoleHelper.Separator();
Console.WriteLine("[Parameters]");
Console.WriteLine("Obsidian Project Path: \"{0}\"", Params.ObsidianProjectPath);
Console.WriteLine("Verbose mode: {0}", Params.Verbose ? "On" : "Off");
ConsoleHelper.Separator();

// Build paths.
Params.SilicaPath = Path.Combine(Params.ObsidianProjectPath, ".silica");
Params.DeployPath = Path.Combine(Params.SilicaPath, "www");
string configFilePath = Path.Combine(Params.SilicaPath, "config.cfg");

// Set output.
if (Directory.Exists(Params.SilicaPath))
{
    Config.TryBackupConfigFile(configFilePath);
    Directory.Delete(Params.SilicaPath, true);
}

if (!Directory.CreateDirectory(Params.SilicaPath).Exists)
{
    Console.Error.WriteLine($"Error: Could not create directory \"{Params.SilicaPath}\"");
    return -1;
}

Config.TryRestoreConfigFile(configFilePath);

// Get user config.
if (Config.CreateFileIfNotExists(configFilePath))
{
    Console.WriteLine("Created default config file.");
    Console.WriteLine($"You can now edit config in \"{configFilePath}\". Press Enter when ready.");
    Console.ReadLine();
    Console.WriteLine("Done editing.");
}
else
{
    Console.WriteLine($"Using existing config file \"{configFilePath}\".");
}

Config.LoadFromFile(configFilePath);

ConsoleHelper.Separator();

//// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP ///

// Print file tree.
// TODO: Split parsing from printing.
Console.WriteLine("[Parsing notes...]");
ConsoleHelper.ParseNotes(Params.ObsidianProjectPath);
ConsoleHelper.Separator();

return 0;

namespace Silica
{
    public static class Params
    {
        // User-defined params.
        internal static string ObsidianProjectPath = string.Empty;
        internal static bool Verbose;
        
        // Computed params.
        internal static string SilicaPath = string.Empty;
        internal static string DeployPath = string.Empty;
    }

    public static class ConsoleHelper
    {
        public static void AppLogo()
        {
            Console.WriteLine();
            Console.WriteLine("    █████████   ███  ████   ███    O b s i d i a n  ");
            Console.WriteLine("    ███░░░░░███ ░░░  ░░███  ░░░    Deployment Tool  ");
            Console.WriteLine("   ░███    ░░░  ████  ░███  ████   ██████   ██████  ");
            Console.WriteLine("   ░░█████████ ░░███  ░███ ░░███  ███░░███ ░░░░░███ "); 
            Console.WriteLine("    ░░░░░░░░███ ░███  ░███  ░███ ░███ ░░░   ███████ ");
            Console.WriteLine("    ███    ░███ ░███  ░███  ░███ ░███  ███ ███░░███ ");
            Console.WriteLine("   ░░█████████  █████ █████ █████░░██████ ░░████████");
            Console.WriteLine("    ░░░░░░░░░  ░░░░░ ░░░░░ ░░░░░  ░░░░░░   ░░░░░░░░ ");
            Console.WriteLine();
        }
        public static void Separator()
        {
            Console.WriteLine("--------------------------------------------------------");
        }
        
        public static void ParseNotes(string path, int depth = 0)
        {
            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                if (Path.GetFileName(dir).StartsWith('.'))
                {
                    continue;
                }

                if (Params.Verbose)
                {
                    Console.WriteLine("{0}{1}", new string(' ', depth * 4), Path.GetRelativePath(Params.ObsidianProjectPath, dir));
                }

                ParseNotes(dir, depth + 1);
            }

            foreach (string file in Directory.EnumerateFiles(path))
            {
                // TODO: Also exclude templates?
                // TODO: Bases support? NOP!!! -> Exclude .base files.
                // TODO: Exclude list for README.md?
                
                // Exclude .obsidian directory (also .git and .silica).
                // TODO: Exclude list (json?)
                if (Path.GetFileName(file).StartsWith('.'))
                {
                    continue;
                }

                if (Params.Verbose)
                {
                    Console.Write("{0}{1}", new string(' ', depth * 4), Path.GetRelativePath(Params.ObsidianProjectPath, file));
                }

                //// Parsing
                
                // Notes
                if (file.EndsWith(".md"))
                {
                    ObNote note = ObNote.Parse(file);
                    try
                    {
                        note.WriteToDisk(Params.DeployPath);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(e.Message);
                    }

                    if (Params.Verbose)
                    {
                        Console.WriteLine($" -> {note}");
                    }
                }
                
                // TODO: Images
                
                // TODO: Other files?
            }
        }
    }
}
