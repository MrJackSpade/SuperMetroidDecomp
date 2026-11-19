using SuperMetroid.Core.Enemies.SpacePirates;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>
/// Confirms that the chance a wall Space Pirate climbs up (#2479) and the chance a ninja Space Pirate about to divekick
/// draws again (#2510) decide every one of the 65536 samples exactly as the tests of the sample's lowest bits they replace
/// did.
/// </summary>
internal static class SpacePirateRandomChancesVerification
{
    /// <summary>Compares every sample's named answers with the bit tests they replace.</summary>
    internal static void VerifySpacePirateRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
        {
            AssertSameAt((sample & 1) != 0, SpacePirateDecisions.ClimbsUp.IsTakenBy(sample), "wall pirate climbs up", sample);
            AssertSameAt((sample & 3) == 0, SpacePirateDecisions.DivekickDrawnAgain.IsTakenBy(sample), "ninja pirate draws its divekick again", sample);
        }

        Console.WriteLine("Space Pirate random chances: all 65536 samples decide as the bit tests they replace.");
    }
}
