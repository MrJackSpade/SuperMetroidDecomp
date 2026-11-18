using SuperMetroid.Core.Menus;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>
/// The fixture's player host: it compares rows held in the framework's value holders, a tuple key, tuple equality, a
/// nullable set element and a key-value pair, whose equality compares what they hold, so each row's value, which
/// nothing reads by name, is what tells two of them apart.
/// </summary>
internal static class Program
{
    /// <summary>Prints what comparing the held rows finds.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            var anchors = new Dictionary<(TupleKeyedRow Row, int Page), int>
            {
                [(new TupleKeyedRow(1), 1)] = 8,
                [(new TupleKeyedRow(2), 1)] = 16,
            };
            Console.WriteLine(anchors[(new TupleKeyedRow(2), 1)]);
            Console.WriteLine((new TupleComparedRow(1), 1) == (new TupleComparedRow(2), 1));
            var shown = new HashSet<NullableKeyedRow?> { new NullableKeyedRow(1), new NullableKeyedRow(2), null };
            Console.WriteLine(shown.Count);
            KeyValuePair<PairedKeyRow, int>[] pairs = [new(new PairedKeyRow(1), 8), new(new PairedKeyRow(2), 8)];
            Console.WriteLine(pairs.Distinct().Count());
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
