using SuperMetroid.Core.Title;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: it prints the first pan's transform and the first fade.</summary>
internal static class Program
{
    /// <summary>Prints the first pan's phase, offset and zoom, and the first fade's level and length.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            Console.WriteLine(Scenes.FirstPan.Phase);
            Console.WriteLine(Scenes.FirstPan.Position);
            Console.WriteLine(Scenes.FirstPan.Zoom);
            Console.WriteLine(Scenes.FirstFade.Level);
            Console.WriteLine(Scenes.FirstFade.Length);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
