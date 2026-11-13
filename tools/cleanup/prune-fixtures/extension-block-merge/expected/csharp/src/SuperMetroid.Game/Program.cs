using SuperMetroid.AssetExtraction.Lists;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production reads a layout's title and first entry, nothing else.</summary>
internal static class Program
{
    /// <summary>Prints the layout's title and first entry.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            ListLayout layout = new([4, 12]);
            Console.WriteLine($"{ListNames.Title(layout)}: {layout.Entry(0)}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
