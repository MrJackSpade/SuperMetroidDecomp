using SuperMetroid.AssetExtraction.Lists;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production takes a place and reads the slots' count and their places' name.</summary>
internal static class Program
{
    /// <summary>Prints what production reads.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            ListPlaces places = new(4);
            places.Take();
            Console.WriteLine($"{new ListSlots(places).Count} {ListSlots.PlacesName}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
