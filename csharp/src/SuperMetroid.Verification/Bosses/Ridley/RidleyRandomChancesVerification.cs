using SuperMetroid.Core.Bosses.Ridley;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// Confirms that Ridley's named random chances (#1619) and Ceres Ridley's attack choice (#2510) decide every one of the
/// 65536 samples exactly as the expressions they replace did: his pogo fireballs, his tail whips, his tail's curliness, his
/// pogo bounce's heading and the attack Ceres Ridley starts after a hover.
/// </summary>
internal static class RidleyRandomChancesVerification
{
    /// <summary>Ceres Ridley's attacks as the replaced lookup listed them, by the sample's lowest four bits.</summary>
    private static readonly CeresRidleyPhase[] ReplacedCeresAttacks =
    [
        CeresRidleyPhase.FireballMoveToPosition, CeresRidleyPhase.FireballMoveToPosition,
        CeresRidleyPhase.FireballMoveToPosition, CeresRidleyPhase.FireballMoveToPosition,
        CeresRidleyPhase.LungeSetup, CeresRidleyPhase.LungeSetup,
        CeresRidleyPhase.LungeSetup, CeresRidleyPhase.LungeSetup,
        CeresRidleyPhase.LungeSetup, CeresRidleyPhase.FireballMoveToPosition,
        CeresRidleyPhase.SwoopSetup, CeresRidleyPhase.SwoopSetup,
        CeresRidleyPhase.SwoopSetup, CeresRidleyPhase.SwoopSetup,
        CeresRidleyPhase.SwoopSetup, CeresRidleyPhase.FireballMoveToPosition,
    ];

    /// <summary>Compares every sample's named answers with the threshold expressions they replace.</summary>
    internal static void VerifyRidleyRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
        {
            AssertSameAt((sample & 255) >= 128, RidleyRandomDefinitions.WallSweepFireballs.IsTakenBy(sample), "pogo fireballs", sample);
            AssertSameAt((sample & 255) >= 240, RidleyRandomDefinitions.TailWhips.IsTakenBy(sample), "tail whip", sample);
            AssertSameAt(((sample >> 8) & 255) == 255, RidleyRandomDefinitions.TailCurlinessChanges.IsTakenBy(sample), "tail curliness change", sample);
            AssertSameAt((sample & 7) < 4, RidleyRandomDefinitions.OrdinaryAttackPogos.IsTakenBy(sample), "ordinary attack pogo", sample);
            AssertSameAt((sample & 7) < 6, RidleyRandomDefinitions.DamageBoostingAttackLunges.IsTakenBy(sample), "damage-boosting attack lunge", sample);
            AssertSameAt(ReplacedCeresAttacks[sample & 15], CeresRidleyAttackDefinitions.Attack.Choose(sample), "Ceres Ridley attack", sample);
            bool headsAway = RidleyRandomDefinitions.PogoBounceHeadsAwayFromSamus.IsTakenBy(sample);
            foreach (bool headsTowardSamus in (ReadOnlySpan<bool>)[true, false])
            {
                bool replaced = headsTowardSamus ? sample < 1365 : sample >= 1365;
                AssertSameAt(replaced, headsTowardSamus ? headsAway : !headsAway, 
                    headsTowardSamus ? "pogo bounce reversal heading toward Samus" : "pogo bounce reversal heading away from Samus", sample);
            }
        }

        Console.WriteLine("Ridley random chances: all 65536 samples decide as the expressions they replace.");
    }
}
