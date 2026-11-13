using SuperMetroid.AssetExtraction.Lists;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production reads a layout's identity and a list's place, nothing else.</summary>
internal static class Program
{
    /// <summary>Prints the layout's identity and its second list's place.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            ListLayout layout = new([4, 12]);
            ListEntry entry = new(layout, 1);
            Console.WriteLine($"{layout.Identity} {entry.Index} of {entry.Layout.Count}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
