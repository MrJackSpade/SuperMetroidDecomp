using SuperMetroid.Core.Bosses.Phantoon;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>
/// Confirms that Phantoon's named count phases (#1619) read every count from 0 through 65535 exactly as the masks they
/// replace did: which rage waves fire eight flames and which dying fade cycles fade him back in, and that every random
/// sample takes his figure eight's chances as the bit tests they replace did.
/// </summary>
internal static class PhantoonCounterPhasesVerification
{
    /// <summary>Compares every count's named phase with the mask expression it replaces.</summary>
    internal static void VerifyPhantoonCounterPhases()
    {
        for (int count = 0; count <= ushort.MaxValue; count++)
        {
            AssertSameAt((count & 1) == 0, !PhantoonCadenceDefinitions.OddRageWave.IsInSecondHalfAt(count), "seven-flame rage wave", count);
            AssertSameAt((count & 1) != 0, PhantoonCadenceDefinitions.DyingFadeIn.IsInSecondHalfAt(count), "dying fade in", count);
            // A random sample takes his figure eight's chances as the cartridge tested its lowest bit (#2470).
            AssertSameAt((count & 1) != 0, PhantoonRandomDefinitions.FigureEightRunsReversed.IsTakenBy(count), "reversed figure eight", count);
            AssertSameAt((count & 1) != 0, PhantoonRandomDefinitions.FigureEightStartsRight.IsTakenBy(count), "figure eight starting right", count);
        }

        Console.WriteLine("Phantoon counter phases: every count from 0 through 65535 reads as the mask expressions they replace.");
    }
}
