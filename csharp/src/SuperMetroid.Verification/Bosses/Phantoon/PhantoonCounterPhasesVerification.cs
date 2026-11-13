using SuperMetroid.Core.Bosses.Phantoon;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>
/// Confirms that Phantoon's named count phases (#1619) read every count from 0 through 65535 exactly as the masks they
/// replace did: which rage waves fire eight flames and which dying fade cycles fade him back in.
/// </summary>
internal static class PhantoonCounterPhasesVerification
{
    /// <summary>Compares every count's named phase with the mask expression it replaces.</summary>
    internal static void VerifyPhantoonCounterPhases()
    {
        for (int count = 0; count <= ushort.MaxValue; count++)
        {
            AssertSameAt((count & 1) == 0, !PhantoonCadenceDefinitions.OddRageWave.IsSetIn(count), "seven-flame rage wave", count);
            AssertSameAt((count & 1) != 0, PhantoonCadenceDefinitions.DyingFadeIn.IsSetIn(count), "dying fade in", count);
        }

        Console.WriteLine("Phantoon counter phases: every count from 0 through 65535 reads as the mask expressions they replace.");
    }
}
