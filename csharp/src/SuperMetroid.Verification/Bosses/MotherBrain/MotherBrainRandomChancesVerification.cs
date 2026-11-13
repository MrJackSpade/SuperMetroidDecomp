using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.MotherBrain;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Confirms that Mother Brain's named random chances and weighted choices (#1619) decide every one of the 65536 samples
/// exactly as the threshold comparisons they replace did, and that her brain's shake step reads every shake count as the
/// masking it replaces did.
/// </summary>
internal static class MotherBrainRandomChancesVerification
{
    /// <summary>The attacks the old threshold chain's choices zero through three named, in order.</summary>
    private static readonly MotherBrainSecondPhaseAttack[] ThresholdAttacks =
    [
        MotherBrainSecondPhaseAttack.Neutral,
        MotherBrainSecondPhaseAttack.FourOnionRings,
        MotherBrainSecondPhaseAttack.Laser,
        MotherBrainSecondPhaseAttack.Bomb,
    ];

    /// <summary>Compares every sample's named answers with the threshold expressions they replace.</summary>
    internal static void VerifyMotherBrainRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
        {
            int roll4096 = sample & 4095;
            int roll256 = sample & 255;
            AssertSameAt(sample < 4096, MotherBrainRandomDefinitions.AttacksAtFullStrength.IsTakenBy(sample), "attack at full strength", sample);
            AssertSameAt(sample < 8192, MotherBrainRandomDefinitions.WalksWhenWounded.IsTakenBy(sample), "walk when wounded", sample);
            AssertSameAt(sample >= 40960, MotherBrainRandomDefinitions.AttacksWhenWounded.IsTakenBy(sample), "attack when wounded", sample);
            AssertSameAt(roll256 >= 128, MotherBrainRandomDefinitions.AttackPresses.IsTakenBy(sample), "pressing attack", sample);
            AssertSameAt(ThresholdAttack(roll256, 16, 32, 208), MotherBrainRandomDefinitions.NearAttack.Choose(sample), "near attack", sample);
            AssertSameAt(ThresholdAttack(roll256, 64, 128, 192), MotherBrainRandomDefinitions.FarAttack.Choose(sample), "far attack", sample);
            AssertSameAt(!(sample < 65408), MotherBrainRandomDefinitions.BombWalkStaysPut.IsTakenBy(sample), "bomb walk stays put", sample);
            AssertSameAt(sample >= 24576 ? 64 : 96, MotherBrainRandomDefinitions.BombWalkShortRetreat.IsTakenBy(sample) ? 96 : 64, "bomb walk target", sample);
            AssertSameAt(sample < 32768, BossRandomRolls.InLowerHalf(sample), "bomb fired without crouching", sample);
            AssertSameAt(roll4096 >= 4032, MotherBrainForwardWalkDefinitions.ForwardWalk.IsTakenBy(sample), "forward walk", sample);
            AssertSameAt(sample < 61440, MotherBrainRandomDefinitions.NeutralPhase2Holds.IsTakenBy(sample), "phase-two neutral hold", sample);
            AssertSameAt(roll4096 < 3776, MotherBrainRandomDefinitions.NeutralPhase3Holds.IsTakenBy(sample), "phase-three neutral hold", sample);
            AssertSameAt(roll4096 >= 4064, MotherBrainRandomDefinitions.DroolBursts.IsTakenBy(sample), "drool burst", sample);
            AssertSameAt(roll4096 >= 4000, MotherBrainRandomDefinitions.BabyMetroidCries.IsTakenBy(sample), "baby metroid cry", sample);
            AssertSameAt(roll4096 >= 4064, MotherBrainRandomDefinitions.ShitroidHoverRestarts.IsTakenBy(sample), "Shitroid hover restart", sample);
            AssertSameAt(roll4096 < 4000, MotherBrainRandomDefinitions.FinishOffHoldsBack.IsTakenBy(sample), "finish-off holds back", sample);
            AssertSameAt(!(roll4096 < 4080), MotherBrainRandomDefinitions.FinishOffBombs.IsTakenBy(sample), "finish-off bomb", sample);
            AssertSameAt(!(roll256 < 192), MotherBrainRandomDefinitions.PostureChanges.IsTakenBy(sample), "posture change", sample);
            AssertSameAt(roll256 < 128, MotherBrainRandomDefinitions.ThirdPhaseBombs.IsTakenBy(sample), "phase-three bomb", sample);
            AssertSameAt(sample < 16384, MotherBrainRandomDefinitions.FakeDeathDustSmokes.IsTakenBy(sample), "fake death dust smoke", sample);
            AssertSameAt(sample < 16384, MotherBrainRandomDefinitions.EscapeDoorDustSmokes.IsTakenBy(sample), "escape door dust smoke", sample);
            AssertSameAt(sample < 16384 ? 0 : sample < 57344 ? 1 : 2, MotherBrainRandomDefinitions.MixedDeathExplosionAnimation.Choose(sample),
                "mixed death explosion program", sample);

            MotherBrainRandomSample seed = MotherBrainRandomSample.FromGenerator(sample);
            AssertSameAt(roll256 < 128, seed.Takes(MotherBrainRandomDefinitions.ThirdPhaseBombs), "sampled phase-three bomb", sample);
            AssertSameAt(!(roll256 < 192), seed.Takes(MotherBrainRandomDefinitions.PostureChanges), "sampled posture change", sample);
            AssertSameAt(roll4096 < 4000, seed.Takes(MotherBrainRandomDefinitions.FinishOffHoldsBack), "sampled finish-off holds back", sample);
            AssertSameAt(!(roll4096 < 4080), seed.Takes(MotherBrainRandomDefinitions.FinishOffBombs), "sampled finish-off bomb", sample);
        }

        for (int shake = short.MinValue; shake <= ushort.MaxValue; shake++)
            AssertSameAt((shake & 6) >> 1, MotherBrainCadenceDefinitions.BrainShakeStep.Of(shake), "brain shake step", shake);

        int smokePlaces = MotherBrainBehavior.MotherBrainFallingTubeSmokeXOffsets.Length;
        for (int smoke = 0; smoke < smokePlaces; smoke++)
            AssertSameAt((smoke + 1) & 3, (smoke + 1) % smokePlaces, "falling tube smoke place", smoke);

        Console.WriteLine("Mother Brain random chances: all 65536 samples, every shake count and every tube smoke place decide as the threshold and mask expressions they replace.");
    }

    /// <summary>The attack the old chain of three ascending thresholds chose for <paramref name="roll"/>.</summary>
    private static MotherBrainSecondPhaseAttack ThresholdAttack(int roll, int first, int second, int third) =>
        ThresholdAttacks[roll < first ? 0 : roll < second ? 1 : roll < third ? 2 : 3];
}
