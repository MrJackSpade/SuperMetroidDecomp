using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>The fixture's player host: production reads a goop step's phase and exit and two crates' labels.</summary>
internal static class Program
{
    /// <summary>Prints what production reads.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            var step = new SuperMetroid.Game.Steps.GoopStep(2, 5);
            Console.WriteLine($"{step.Phase} {step.Exit} {new SuperMetroid.Game.Steps.Crate("a").Label} {new SuperMetroid.Game.Steps.Crate().Label} {new SuperMetroid.Game.Steps.Ticket(" x ").Length}");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
