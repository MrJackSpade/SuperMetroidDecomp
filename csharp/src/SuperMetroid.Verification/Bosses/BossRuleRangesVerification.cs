using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;

namespace SuperMetroid.Verification.Bosses;

/// <summary>
/// Checks the boss compatibility rules #1620 named and the death explosion rules it made return their explosion, each over
/// its whole input range against the original's arithmetic.
/// </summary>
internal static class BossRuleRangesVerification
{
    /// <summary>The samples the shared random value takes.</summary>
    private const int RandomRange = 65536;

    /// <summary>
    /// Every sample against every threshold the bosses roll with (and the range's ends): a roll passes exactly when the
    /// sample less the threshold reads as not negative in sixteen bits, as the original's sign test reads it.
    /// </summary>
    internal static void VerifyBossRandomRolls()
    {
        foreach (int threshold in new[] { 0, 1, 2048, 4096, 32767 })
        {
            for (int sample = 0; sample < RandomRange; sample++)
            {
                bool original = unchecked((short)(sample - threshold)) >= 0;
                if (BossRandomRolls.InHalfFrom(sample, threshold) != original)
                    throw new InvalidOperationException($"Roll from {threshold} differs from the sign test at sample {sample}.");
                if (threshold == 0 && BossRandomRolls.InLowerHalf(sample) != original)
                    throw new InvalidOperationException($"The lower-half roll differs from the sign test at sample {sample}.");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => BossRandomRolls.InHalfFrom(RandomRange, 0), "a sample past the range is refused");
        AssertThrows<ArgumentOutOfRangeException>(() => BossRandomRolls.InHalfFrom(0, 32768), "a threshold past half the range is refused");
        Console.WriteLine("  Boss random rolls: every sample and threshold matches the sign test.");
    }

    /// <summary>
    /// The death explosion a leftover number names, over every number a touch's health or a burst's place leaves: zero
    /// through four name the explosions in their authored order and anything past them, or no number, the small one; a
    /// Powamp's first three spike places select small, normal and big.
    /// </summary>
    internal static void VerifyDeathExplosionRules()
    {
        LeftoverNumberDeathExplosion<EnemyDeathExplosion> leftover = EnemyDeathExplosionDefinitions.LeftoverNumbered;
        EnemyDeathExplosion[] expectedByNumber =
        [
            EnemyDeathExplosion.Small, EnemyDeathExplosion.SamusContact, EnemyDeathExplosion.Normal,
            EnemyDeathExplosion.FakeKraid, EnemyDeathExplosion.Big,
        ];
        for (int number = 0; number < 1024; number++)
        {
            EnemyDeathExplosion expected = number < expectedByNumber.Length ? expectedByNumber[number] : EnemyDeathExplosion.Small;
            AssertEqual(expected, leftover.Named(number), $"a leftover number {number}");
        }
        AssertEqual(EnemyDeathExplosion.Small, leftover.Named(null), "a leftover that is no number");
        AssertEqual(EnemyDeathExplosion.Small, SuperMetroid.Verification.Enemies.Powamp.PowampBurstExplosionVerification.ExplosionAfter(null), "a burst into a full pool");
        for (int place = 0; place < 18; place++)
        {
            EnemyDeathExplosion expected = place switch
            {
                0 => EnemyDeathExplosion.Small,
                1 => EnemyDeathExplosion.Normal,
                2 => EnemyDeathExplosion.Big,
                _ => EnemyDeathExplosion.Small,
            };
            AssertEqual(expected, SuperMetroid.Verification.Enemies.Powamp.PowampBurstExplosionVerification.ExplosionAfter(place), $"a last spike in place {place}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => leftover.Named(-1), "a negative number is refused");
        Console.WriteLine("  Death explosion rules: every leftover number and spike place selects the original's explosion.");
    }
}
