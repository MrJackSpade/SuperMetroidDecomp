using SuperMetroid.Core;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: it shows a note and reports an unsupported state by its identity.</summary>
internal static class Program
{
    /// <summary>Prints a note, and the identity of any unsupported state its showing raised.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            Console.WriteLine(new Note("hello").Text);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception is UnsupportedState unsupported ? unsupported.Identity : exception.ToString());
            return 1;
        }
    }
}
