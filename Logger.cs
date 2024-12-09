namespace ShellyTnhToProm;

internal static class Logger
{
    internal static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now.ToLocalTime():s}]{message}");
    }
}
