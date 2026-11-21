using SuperMetroid.Core.Figures;
using SuperMetroid.Core.Liquids;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.Game;

/// <summary>
/// The fixture's player host: a presentation's mode and a liquid's motion start at their enums' zero members, which no
/// code names (the shapes of <c>SamusPresentationMode.Gameplay</c> and <c>LiquidMotionStage.AwaitingFirstPass</c>, which
/// the cleanups of 716ee318d3 and 44add85c59 removed, #2608).
/// </summary>
internal static class Program
{
    /// <summary>Prints what production reads.</summary>
    private static int Main()
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            var presentation = new FigurePresentation();
            Console.WriteLine(presentation.Mode == FigurePresentationMode.Cinematic);
            presentation.ShowInCinematic();
            Console.WriteLine(presentation.Mode == FigurePresentationMode.Cinematic);
            var liquid = new Liquid();
            Console.WriteLine(liquid.Motion.IsMoving);
            liquid.Begin();
            Console.WriteLine(liquid.Motion.IsMoving);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
