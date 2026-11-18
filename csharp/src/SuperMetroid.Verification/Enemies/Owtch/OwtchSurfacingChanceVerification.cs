using SuperMetroid.Core.Enemies.Owtch;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Owtch;

/// <summary>
/// Confirms that the chance a surfacing Owtch walks off right (#2479) decides every one of the 65536 samples exactly as
/// the test of the sample's ninth bit it replaces did.
/// </summary>
internal static class OwtchSurfacingChanceVerification
{
    /// <summary>Compares every sample's named answer with the bit test it replaces.</summary>
    internal static void VerifyOwtchSurfacingChance()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 256) != 0, OwtchMovementDefinitions.SurfacesMovingRight.IsTakenBy(sample), "surfaces moving right", sample);

        Console.WriteLine("Owtch surfacing chance: all 65536 samples decide as the bit test it replaces.");
    }
}
