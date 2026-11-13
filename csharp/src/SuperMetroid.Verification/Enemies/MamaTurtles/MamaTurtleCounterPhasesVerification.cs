using SuperMetroid.Core.Enemies.MamaTurtles;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.MamaTurtles;

/// <summary>
/// Confirms that the named phase of a Mama Turtle's height as she leaves her shell (#1619) sidesteps at every height
/// exactly as the mask it replaces did: right on even heights, left on odd ones.
/// </summary>
internal static class MamaTurtleCounterPhasesVerification
{
    /// <summary>Compares every height, negative ones included, with the replaced mask expression.</summary>
    internal static void VerifyMamaTurtleCounterPhases()
    {
        for (int y = short.MinValue; y <= ushort.MaxValue; y++)
            AssertSameAt((y & 1) == 0 ? 1 : -1, MamaTurtleCadenceDefinitions.LeavingShellSidestepsLeft.IsSetIn(y) ? -1 : 1, "leaving-shell sidestep", y);

        Console.WriteLine("Mama Turtle counter phases: every height sidesteps as the mask it replaces.");
    }
}
