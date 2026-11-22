using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production reads a basket's count, its and a shelf's names and a box's size.</summary>
internal static class Program
{
    /// <summary>Prints what production reads.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            var basket = new SuperMetroid.Game.Counting.Basket();
            foreach (SuperMetroid.Game.Counting.INamed named in new SuperMetroid.Game.Counting.INamed[] { basket, new SuperMetroid.Game.Counting.Shelf() })
                Console.WriteLine(named.Name);
            Console.WriteLine($"{basket.Count} {new SuperMetroid.Game.Counting.Box().Size}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
