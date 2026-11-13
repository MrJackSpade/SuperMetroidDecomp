using SuperMetroid.Core.Bosses.Ridley;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// Confirms that Ridley's named random chances (#1619) decide every one of the 65536 samples exactly as the threshold
/// comparisons they replace did: his pogo fireballs, his tail whips, his tail's curliness and his pogo bounce's heading.
/// </summary>
internal static class RidleyRandomChancesVerification
{
    /// <summary>Compares every sample's named answers with the threshold expressions they replace.</summary>
    internal static void VerifyRidleyRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
        {
            AssertSameAt((sample & 255) >= 128, RidleyRandomDefinitions.PogoFireballs.IsTakenBy(sample), "pogo fireballs", sample);
            AssertSameAt((sample & 255) >= 240, RidleyRandomDefinitions.TailWhips.IsTakenBy(sample), "tail whip", sample);
            AssertSameAt(((sample >> 8) & 255) == 255, RidleyRandomDefinitions.TailCurlinessChanges.IsTakenBy(sample), "tail curliness change", sample);
            bool headsAway = RidleyRandomDefinitions.PogoBounceHeadsAwayFromSamus.IsTakenBy(sample);
            foreach (bool headsTowardSamus in (ReadOnlySpan<bool>)[true, false])
            {
                bool replaced = headsTowardSamus ? sample < 1365 : sample >= 1365;
                AssertSameAt(replaced, headsTowardSamus ? headsAway : !headsAway, 
                    headsTowardSamus ? "pogo bounce reversal heading toward Samus" : "pogo bounce reversal heading away from Samus", sample);
            }
        }

        Console.WriteLine("Ridley random chances: all 65536 samples decide as the threshold expressions they replace.");
    }
}
