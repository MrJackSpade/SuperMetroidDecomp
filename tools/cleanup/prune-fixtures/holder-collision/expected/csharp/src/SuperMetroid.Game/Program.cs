using SuperMetroid.AssetExtraction.Lists;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production reads a list entry and nothing else of the layout.</summary>
internal static class Program
{
    /// <summary>Prints the first list's entry.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            Console.WriteLine(new ListLayout([4], [2]).Position(0));
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
