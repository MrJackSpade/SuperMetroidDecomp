using SuperMetroid.Core.Areas.Ceres;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// Confirms that the Ceres escape door's named random chances (#1619) decide every one of the 65536 samples exactly as the
/// threshold comparisons they replace did: its earthquake's strength and its destruction's explosion animation.
/// </summary>
internal static class CeresDoorRandomChancesVerification
{
    /// <summary>Compares every sample's named answers with the threshold expressions they replace.</summary>
    internal static void VerifyCeresDoorRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
        {
            AssertSameAt((sample & 4095) < 128, CeresDoorQuakeDefinitions.StrongQuake.IsTakenBy(sample), "strong quake", sample);
            AssertSameAt(sample < 16384, CeresDoorQuakeDefinitions.RumbleExplosion.IsTakenBy(sample), "rumble explosion animation", sample);
        }

        Console.WriteLine("Ceres door random chances: all 65536 samples decide as the threshold expressions they replace.");
    }
}
