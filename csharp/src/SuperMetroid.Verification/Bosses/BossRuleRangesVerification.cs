using SuperMetroid.Core.Enemies;
using SuperMetroid.Verification.Harness;

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
    /// The death explosion rules over every health and place: zero through four name the explosions in their authored
    /// order and anything past them the small one; a Powamp's first three spike places select small, normal and big.
    /// </summary>
    internal static void VerifyDeathExplosionRules()
    {
        IReadOnlyList<EnemyDeathExplosion> order = EnemyDeathExplosionDefinitions.AuthoredOrder;
        EnemyDeathExplosion[] expectedByNumber =
        [
            EnemyDeathExplosion.Small, EnemyDeathExplosion.SamusContact, EnemyDeathExplosion.Normal,
            EnemyDeathExplosion.MiniKraid, EnemyDeathExplosion.Big,
        ];
        for (int health = 0; health < 1024; health++)
        {
            EnemyDeathExplosion expected = health < expectedByNumber.Length ? expectedByNumber[health] : EnemyDeathExplosion.Small;
            AssertEqual(expected, FirefleaTouchDeathExplosion.AfterSurvivedTouch(health, order), $"a Fireflea left {health} health");
            AssertEqual(expected, KraidNailTouchDeathExplosion.AfterTouch(health, order), $"a fingernail left {health} health");
        }
        AssertEqual(EnemyDeathExplosion.Small, FirefleaTouchDeathExplosion.AfterKillingTouch(order), "a Fireflea killed by the touch dies again small");
        AssertEqual(EnemyDeathExplosion.Small, PowampBurstExplosion.AfterBurst(null, order), "a burst into a full pool");
        for (int place = 0; place < 18; place++)
        {
            EnemyDeathExplosion expected = place switch
            {
                0 => EnemyDeathExplosion.Small,
                1 => EnemyDeathExplosion.Normal,
                2 => EnemyDeathExplosion.Big,
                _ => EnemyDeathExplosion.Small,
            };
            AssertEqual(expected, PowampBurstExplosion.AfterBurst(place, order), $"a last spike in place {place}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => FirefleaTouchDeathExplosion.AfterSurvivedTouch(-1, order), "negative health is refused");
        AssertTrue(KilledFingernailStops.SiblingTookAlternateSpawn("waiting", static _ => true), "a living sibling's alternate spawn is read");
        AssertTrue(!KilledFingernailStops.SiblingTookAlternateSpawn<string>(null, static _ => true), "a killed sibling took no alternate spawn");
        AssertTrue(!KilledFingernailStops.SiblingTookAlternateSpawn("launched", static _ => false), "a sibling that took the ordinary spawn");
        Console.WriteLine("  Death explosion rules: every health and spike place selects the original's explosion; a killed fingernail took no alternate spawn.");
    }
}
