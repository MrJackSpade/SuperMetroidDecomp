using SuperMetroid.Core;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: it prints what the left cue does.</summary>
internal static class Program
{
    /// <summary>Prints the left cue's kind.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            Console.WriteLine(Cues.Left.Kind);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
