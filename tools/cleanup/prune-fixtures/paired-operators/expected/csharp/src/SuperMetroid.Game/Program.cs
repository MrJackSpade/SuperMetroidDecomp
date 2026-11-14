using SuperMetroid.Core.Lists;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production tests one operator of each of two pairs and reads a row's width.</summary>
internal static class Program
{
    /// <summary>Prints what production reads.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            ListLength shorter = new(1);
            ListLength longer = new(2);
            Console.WriteLine(shorter < longer);
            Console.WriteLine(shorter.Equals(longer));
            Console.WriteLine(new ListRow().Width);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
