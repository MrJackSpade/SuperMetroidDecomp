using SuperMetroid.AssetExtraction.Lists;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production reads a list's second item and a wide list's stride.</summary>
internal static class Program
{
    /// <summary>Prints what production reads.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            Console.WriteLine($"{new StridedList(2).Second} {new WideList().stride}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
