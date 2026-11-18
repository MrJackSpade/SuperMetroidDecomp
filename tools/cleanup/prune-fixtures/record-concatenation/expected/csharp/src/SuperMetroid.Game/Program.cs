using SuperMetroid.Core.Menus;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>
/// The fixture's player host: it concatenates a row with text, which formats the row through the record's synthesized
/// text, so the row's value, which nothing reads by name, is part of what it writes.
/// </summary>
internal static class Program
{
    /// <summary>Writes the row out.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            Console.WriteLine("selected " + new LabelledRow(2));
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
