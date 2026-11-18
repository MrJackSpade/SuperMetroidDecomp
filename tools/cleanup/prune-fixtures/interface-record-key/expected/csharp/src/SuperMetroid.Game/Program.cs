using SuperMetroid.Core.Menus;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>
/// The fixture's player host: it keys the cursor's anchors by the interface the row records implement, so each row's
/// value, which nothing reads by name, is what tells two keys apart.
/// </summary>
internal static class Program
{
    /// <summary>Prints the anchor and page of one row, which needs every row to be a key of its own.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            var anchors = new Dictionary<IMenuRow, int>
            {
                [new FirstPageMenuRow(1)] = 8,
                [new FirstPageMenuRow(2)] = 16,
            };
            Console.WriteLine(anchors[new FirstPageMenuRow(2)] + anchors.Keys.First().Page);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
