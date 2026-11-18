using SuperMetroid.Core.Enemies.SpacePirates;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>
/// Confirms that the chance a wall Space Pirate climbs up (#2479) decides every one of the 65536 samples exactly as the
/// test of the sample's lowest bit it replaces did.
/// </summary>
internal static class SpacePirateRandomChancesVerification
{
    /// <summary>Compares every sample's named answer with the bit test it replaces.</summary>
    internal static void VerifySpacePirateRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 1) != 0, SpacePirateDecisions.ClimbsUp.IsTakenBy(sample), "wall pirate climbs up", sample);

        Console.WriteLine("Space Pirate random chances: all 65536 samples decide as the bit test they replace.");
    }
}
