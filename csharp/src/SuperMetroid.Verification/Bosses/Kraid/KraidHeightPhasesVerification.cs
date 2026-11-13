using SuperMetroid.Core.Bosses.Kraid;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// Confirms that the named phases Kraid's height paces as he rises and sinks (#1619) read every height exactly as the
/// masks they replace did: the sway's direction in both rises, the heights at which a ceiling rock drops and the rows of
/// his sinking schedule.
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
            AssertSameAt((y & 3) == 0, KraidCadenceDefinitions.CeilingRockEveryFourthRow.IsDue(y), "ceiling rock height", y);
            AssertSameAt((y & 7) == 0, KraidCadenceDefinitions.SinkRowEveryEighthPixel.IsDue(y), "sinking schedule row", y);
        }

        Console.WriteLine("Kraid height phases: every height sways, drops ceiling rocks and reaches sinking rows as the masks they replace.");
    }
}
