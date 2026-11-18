using SuperMetroid.Core.Menus;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>
/// The fixture's player host: it keys the cursor's anchors by the base menu row, whose rows differ only in a value
/// held five records deep (derived row, its row's hierarchy, a derived row there, its button's hierarchy, a derived
/// button), which comparing the keys reads however deep it is held.
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
                [Row(1)] = 8,
                [Row(2)] = 16,
            };
            Console.WriteLine(anchors[Row(2)]);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    /// <summary>The controller row assigning button <paramref name="number"/>.</summary>
    private static MenuRow.OnControllerPage Row(int number) =>
        new MenuRow.OnControllerPage(new ControllerRow.AssignsButton(new ControllerButton.Numbered(number)));
}
