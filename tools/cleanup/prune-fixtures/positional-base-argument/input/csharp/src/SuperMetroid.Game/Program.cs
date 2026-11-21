using SuperMetroid.Core.Bosses;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: it shows a band and prints the shade the boss's parts show.</summary>
internal static class Program
{
    /// <summary>Prints the parts' shade after a deep band, and a pale glow's strength.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            var colours = new Colours();
            colours.Show(Tint.Deep);
            Console.WriteLine(colours.PartsShade);
            Console.WriteLine(Colours.GlowStrength);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
