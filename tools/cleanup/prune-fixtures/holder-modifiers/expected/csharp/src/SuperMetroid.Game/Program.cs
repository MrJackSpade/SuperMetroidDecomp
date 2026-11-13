using SuperMetroid.Desktop.Lists;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production reads widths, spans, a cursor and doubled entries, nothing else.</summary>
internal static class Program
{
    /// <summary>Prints what production reads.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            ListCursor cursor = new();
            cursor.Advance();
            ListSpan span = new(2, 3);
            Console.WriteLine($"{new WideListKind().Width} {span.Start + span.Length} {cursor.Index} {ListArithmetic.Double(4)}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
