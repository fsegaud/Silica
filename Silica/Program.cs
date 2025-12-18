using Silica;

// Get parameters.
if (args.Length == 0)
{
    ConsoleHelper.Error("Use : Silica.exe <ObsidianProjectPath> [-v]");
    return -1;
}

Params.ObsidianProjectPath = args[0];
if (!Path.Exists(Params.ObsidianProjectPath))
{
    ConsoleHelper.Error($"Error: Could not find \"{Params.ObsidianProjectPath}\"");
    return -1;
}

Params.Verbose = args.Contains("-v");

// Print parameters
ConsoleHelper.AppLogo(); 
ConsoleHelper.Disclaimer();
ConsoleHelper.Separator();
ConsoleHelper.Title("[Parameters]");
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
    ConsoleHelper.Error($"Error: Could not create directory \"{Params.SilicaPath}\"");
    return -1;
}

Config.TryRestoreConfigFile(configFilePath);

// Get user config.
ConsoleHelper.Title("[Configuration]");
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
Console.WriteLine(Config.Export);

ConsoleHelper.Separator();

//// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP ///

// Print file tree.
// TODO: Split parsing from printing.
ConsoleHelper.Title("[Parsing]");
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
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine();
            Console.WriteLine("    █████████   ███  ████   ███    O b s i d i a n  ");
            Console.WriteLine("    ███░░░░░███ ░░░  ░░███  ░░░    Deployment Tool  ");
            Console.WriteLine("   ░███    ░░░  ████  ░███  ████   ██████   ██████  ");
            Console.WriteLine("   ░░█████████ ░░███  ░███ ░░███  ███░░███ ░░░░░███ ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("    ░░░░░░░░███ ░███  ░███  ░███ ░███ ░░░   ███████ ");
            Console.WriteLine("    ███    ░███ ░███  ░███  ░███ ░███  ███ ███░░███ ");
            Console.WriteLine("   ░░█████████  █████ █████ █████░░██████ ░░████████");
            Console.WriteLine("    ░░░░░░░░░  ░░░░░ ░░░░░ ░░░░░  ░░░░░░   ░░░░░░░░ ");
            Console.WriteLine();
            Console.ResetColor();
        }

        public static void Disclaimer()
        {
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("╔════════════════════[ DISCLAIMER ]════════════════════╗");
            Console.WriteLine("║     Obsidian tables are not currently supported.     ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }
        public static void Separator()
        {
            Console.WriteLine("--------------------------------------------------------");
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine(message);
            Console.ResetColor();
        }
        
        public static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        
        public static void Title(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        
        public static void ParseNotes(string path, int depth = 0)
        {
            foreach (string dir in Directory.EnumerateDirectories(path))
            {
                if (Config.Export.ExcludedFiles.Contains(Path.GetFileName(dir)))
                {
                    if (Params.Verbose) Warning($"{new string(' ', depth * 4)}Excluded \"{dir}\"");
                    continue;
                }

                if (Params.Verbose) Console.WriteLine("{0}{1}", new string(' ', depth * 4), Path.GetRelativePath(Params.ObsidianProjectPath, dir));

                ParseNotes(dir, depth + 1);
            }

            foreach (string file in Directory.EnumerateFiles(path))
            {
                // TODO: Also exclude templates?
                // TODO: Bases support? NOP!!! -> Exclude .base files.
                
                if (Config.Export.ExcludedFiles.Contains(Path.GetFileName(file)))
                {
                    if (Params.Verbose) Warning($"{new string(' ', depth * 4)}Excluded \"{file}\"");
                    continue;
                }

                if (Path.GetFileName(file).EndsWith(".base"))
                {
                    Error($"{new string(' ', depth * 4)}Skipped \"{Path.GetFileName(file)}\". Bases are not supported.");
                    continue;
                }

                if (Params.Verbose) Console.Write("{0}{1}", new string(' ', depth * 4), Path.GetRelativePath(Params.ObsidianProjectPath, file));

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
                        Error(e.Message);
                    }

                    if (Params.Verbose) Console.WriteLine($" -> {note}");
                }
                
                // TODO: Images
                
                // TODO: Other files?
            }
        }
    }
}
