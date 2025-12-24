using Spectre.Console;

namespace Silica;

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
        Panel panel = new Panel(new Markup("[gold1 invert]     Obsidian tables are not currently supported.     [/]"));
        panel.Header = new PanelHeader("DISCLAIMER");
        panel.Border = BoxBorder.Rounded;
        panel.Padding = new Padding(0);
        panel.BorderStyle = Style.Parse("invert");
        panel.BorderColor(Color.Gold1);
        AnsiConsole.Write(panel);
    }

    public static void Error(string message)
    {
        AnsiConsole.MarkupLine($"[red]{message}[/]");
    }
    
    public static void Warning(string message)
    {
        AnsiConsole.MarkupLine($"[gold1]{message}[/]");
    }
    
    public static void Debug(string message)
    {
        AnsiConsole.MarkupLine($"[magenta]{message}[/]");
    }
    
    public static void Title(string message)
    {
        AnsiConsole.Write(new Rule($"[deepskyblue1]{message}[/]").LeftJustified());
    }
}
