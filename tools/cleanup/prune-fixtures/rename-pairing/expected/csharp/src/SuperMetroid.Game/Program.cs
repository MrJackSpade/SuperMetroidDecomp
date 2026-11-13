using SuperMetroid.AssetExtraction.Lists;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production reads a cursor offset's total.</summary>
internal static class Program
{
    /// <summary>Prints what production reads.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            Console.WriteLine(new CursorOffset(2, 3).Total);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
