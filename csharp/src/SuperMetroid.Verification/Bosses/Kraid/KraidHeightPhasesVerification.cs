using SuperMetroid.Core.Bosses.Kraid;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// Confirms that the named phases Kraid's height paces as he rises (#1619) read every height exactly as the
/// masks they replace did: the sway's direction in both rises and the heights at which a ceiling rock drops.
/// </summary>
internal static class KraidHeightPhasesVerification
{
    /// <summary>Compares every height, negative ones included, with the replaced mask expressions.</summary>
    internal static void VerifyKraidHeightPhases()
    {
        for (int y = short.MinValue; y <= ushort.MaxValue; y++)
        {
            AssertSameAt((y & 2) == 0 ? 1 : -1, KraidCadenceDefinitions.RiseSwayPhase.IsSetIn(y) ? -1 : 1, "ceiling-break sway", y);
            AssertSameAt((y & 2) == 0 ? -1 : 1, KraidCadenceDefinitions.RiseSwayPhase.IsSetIn(y) ? 1 : -1, "raised-body sway", y);
            AssertSameAt((y & 3) == 0, KraidCadenceDefinitions.CeilingRockRow.IsDue(y), "ceiling rock height", y);
        }

        Console.WriteLine("Kraid height phases: every height sways and drops ceiling rocks as the masks they replace.");
    }
}
