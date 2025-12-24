using Silica;
using Spectre.Console;

// TODO: HTML fragments.
// TODO: Resource manager.
// TODO: Linux support.

// Get parameters.
// TODO: Use Spectre.ConsoleCLI to parse arguments.
if (args.Length == 0)
{
    ConsoleHelper.Error("Use : Silica.exe <ObsidianProjectPath> [-v]");
    return -1;
}

Params.ObsidianProjectPath = args[0];
if (!Path.Exists(Params.ObsidianProjectPath))
{
    ConsoleHelper.Error($"Error: Could not find [cyan]{Params.ObsidianProjectPath}[/]");
    return -1;
}

Params.Verbose = args.Contains("-v");

// Print parameters
ConsoleHelper.AppLogo(); 
ConsoleHelper.Disclaimer();
ConsoleHelper.Title("Parameters");
AnsiConsole.MarkupLine("Obsidian Project Path: [cyan]{0}[/]", Params.ObsidianProjectPath);
AnsiConsole.MarkupLine("Verbose mode: {0}", Params.Verbose ? "[green]On[/]" : "[red]Off[/]");

// Build paths.
Params.SilicaPath = Path.Combine(Params.ObsidianProjectPath, ".silica");
Params.DeployPath = Path.Combine(Params.SilicaPath, "www");
string configFilePath = Path.Combine(Params.SilicaPath, "config.json");

// Set output.
if (Directory.Exists(Params.SilicaPath))
{
    Config.TryBackupConfigFile(configFilePath);
    Directory.Delete(Params.SilicaPath, true);
}

if (!Directory.CreateDirectory(Params.SilicaPath).Exists)
{
    ConsoleHelper.Error($"Error: Could not create directory [cyan]{Params.SilicaPath}[/]");
    return -1;
}

Directory.CreateDirectory(Params.DeployPath);

Config.TryRestoreConfigFile(configFilePath);

// Get user config.
ConsoleHelper.Title("Configuration");
if (Config.CreateFileIfNotExists(configFilePath))
{
    Console.WriteLine("Created default config file.");
    AnsiConsole.MarkupLine($"You can now edit config in [cyan]{configFilePath}[/]. Press Enter when ready.");
    Console.ReadLine();
    Console.WriteLine("Done editing.");
}
else
{
    AnsiConsole.MarkupLine($"Using existing config file [cyan]{configFilePath}[/].");
}

Config.LoadFromFile(configFilePath);
if (Params.Verbose) AnsiConsole.MarkupLine(Config.Export.ToString());

//// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP //// WIP ///

// Print file tree.
// TODO: Split parsing from printing.
ConsoleHelper.Title("Parsing");
Tree root = new Tree(Params.ObsidianProjectPath);
ParseNotes(Params.ObsidianProjectPath, root);
if (Params.Verbose) AnsiConsole.Write(root);

return 0;

static void ParseNotes(string path, IHasTreeNodes root, int depth = 0)
{
    // Directories.
    foreach (string dir in Directory.EnumerateDirectories(path))
    {
        if (Config.Export.ExcludedFiles.Contains(Path.GetFileName(dir)))
        {
            if (Params.Verbose) ConsoleHelper.Warning($"{new string(' ', depth * 4)}Excluded [cyan]{dir}[/].");
            continue;
        }

        TreeNode node = root.AddNode(Path.GetFileName(dir));

        ParseNotes(dir, node, depth + 1);
    }

    // Files.
    foreach (string file in Directory.EnumerateFiles(path))
    {
        // Excludes.
        if (Config.Export.ExcludedFiles.Contains(Path.GetFileName(file)))
        {
            if (Params.Verbose) ConsoleHelper.Warning($"{new string(' ', depth * 4)}Excluded [cyan]{file}[/].");
            continue;
        }

        // Skips.
        if (Path.GetFileName(file).EndsWith(".base"))
        {
            ConsoleHelper.Error($"{new string(' ', depth * 4)}Skipped unsupported [cyan]{Path.GetFileName(file)}[/].");
            continue;
        }

        root.AddNode(Path.GetFileName(file));

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
                AnsiConsole.WriteException(e);
            }
        }

        // TODO: Images

        // TODO: Other files?
    }
}

public static class Params
{
    // User-defined params.
    internal static string ObsidianProjectPath = string.Empty;
    internal static bool Verbose;
        
    // Computed params.
    internal static string SilicaPath = string.Empty;
    internal static string DeployPath = string.Empty;
}
