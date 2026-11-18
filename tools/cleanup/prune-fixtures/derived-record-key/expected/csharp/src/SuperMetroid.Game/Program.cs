using SuperMetroid.Core.Menus;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>
/// The fixture's player host: it keys the cursor's anchors by the base menu row, so each derived row's value, which
/// nothing reads by name, is what tells two keys of one page apart.
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
                [new MenuRow.OnFirstPage(FirstPageRow.Start)] = 8,
                [new MenuRow.OnFirstPage(FirstPageRow.Options)] = 16,
                [new MenuRow.OnSecondPage(SecondPageRow.Reset)] = 24,
                [new MenuRow.OnSecondPage(SecondPageRow.Exit)] = 32,
            };
            Console.WriteLine(anchors[new MenuRow.OnFirstPage(FirstPageRow.Options)]);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
