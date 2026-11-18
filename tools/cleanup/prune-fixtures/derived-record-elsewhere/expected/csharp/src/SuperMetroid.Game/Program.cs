using SuperMetroid.Core.Menus;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>
/// The fixture's player host: it keys the cursor's anchors by the base menu row of the shipped library, and its own
/// rows, which derive from that row in this project, tell the keys apart by a value nothing reads by name.
/// </summary>
internal static class Program
{
    /// <summary>Prints the anchor of one row, which needs every row to be a key of its own.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            var anchors = new Dictionary<MenuRow, int>
            {
                [new HostMenuRow(1)] = 8,
                [new HostMenuRow(2)] = 16,
            };
            Console.WriteLine(anchors[new HostMenuRow(2)]);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
