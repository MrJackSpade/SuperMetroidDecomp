using SuperMetroid.Core;
using SuperMetroid.Core.Lists;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production reads the width of a title's window and the checks' name.</summary>
internal static class Program
{
    /// <summary>Prints what production reads.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            Console.WriteLine($"{ListWindows.Width("Items")} {Ensure.Name}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
