using SuperMetroid.Desktop.Lists;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production reads a layout's count and a meter's level, nothing else.</summary>
internal static class Program
{
    /// <summary>Prints the layout's count and the meter's level.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            Console.WriteLine($"{ListPings.Count(new ListLayout([4, 12]))} {new ListMeter().Level}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
