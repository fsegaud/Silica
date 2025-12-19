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
    
    public static void Debug(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine(message);
        Console.ResetColor();
    }
    
    public static void Title(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
