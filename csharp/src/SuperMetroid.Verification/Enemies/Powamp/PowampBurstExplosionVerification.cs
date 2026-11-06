using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>
/// #1513: a Powamp shot dead explodes with the explosion its burst's last spike's place in the projectile pool selects,
/// the <see cref="PowampBurstExplosion"/> compatibility rule, rather than a number read from its graphics.
/// </summary>
internal static class PowampBurstExplosionVerification
{
    /// <summary>
    /// The rule gives, in the explosions' authored order, the small explosion for the first place, the normal one for the
    /// second, the big one for the third and the small one for any later place or a full pool; a burst fired into a pool
    /// whose last free places are its second through ninth reports the second place, which selects the normal explosion.
    /// </summary>
    internal static void VerifyPowampBurstExplosion()
    {
        AssertEqual(EnemyDeathExplosion.Small, ExplosionAfter(0), "a last spike in the first place selects the small explosion");
        AssertEqual(EnemyDeathExplosion.Normal, ExplosionAfter(1), "a last spike in the second place selects the normal explosion");
        AssertEqual(EnemyDeathExplosion.Big, ExplosionAfter(2), "a last spike in the third place selects the big explosion");
        AssertEqual(EnemyDeathExplosion.Small, ExplosionAfter(3), "a last spike in the fourth place selects the small explosion");
        AssertEqual(EnemyDeathExplosion.Small, ExplosionAfter(17), "a last spike in the pool's last place selects the small explosion");
        AssertEqual(EnemyDeathExplosion.Small, ExplosionAfter(null), "a burst that found the pool full selects the small explosion");

        var enemies = new RoomEnemySystem(new EnemySession(new UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyProjectileSlot[] pool = [.. enemies.Projectiles.EnemyProjectiles];
        // Occupy every place above the ninth and the first, so the eight spikes take the second through ninth places.
        foreach (RoomEnemyProjectileSlot occupied in pool.Where(slot => slot.SlotIndex is 0 or > 8))
            occupied.Kind = RoomEnemyProjectileKind.MiscDustExplosion;
        var body = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
        };
        AssertEqual<int?>(1, enemies.PowampBehavior.SpawnPowampSpikeBurst(body), "the burst's last spike takes the second place");
        AssertEqual(8, pool.Count(slot => slot.Kind == RoomEnemyProjectileKind.PowampSpike), "the burst fires all eight spikes");

        foreach (RoomEnemyProjectileSlot slot in pool)
            slot.Clear();
        foreach (RoomEnemyProjectileSlot occupied in pool.Where(slot => slot.SlotIndex > 4))
            occupied.Kind = RoomEnemyProjectileKind.MiscDustExplosion;
        AssertEqual<int?>(null, enemies.PowampBehavior.SpawnPowampSpikeBurst(body), "a burst finding the pool full before its eighth spike reports none");
        Console.WriteLine("  Powamp burst explosion: the last spike's pool place selects the explosion; a full pool selects the small one.");
    }

    /// <summary>The explosion a Powamp dies with after a burst whose last spike took <paramref name="place"/>.</summary>
    private static EnemyDeathExplosion ExplosionAfter(int? place) =>
        EnemyDeathExplosions.ForLeftoverNumber(PowampBurstExplosion.AuthoredExplosionAfter(place));

    /// <summary>Runs the Powamp burst explosion rule check.</summary>
    [VerificationCommand("--powamp-burst-explosion")]
    internal static int PowampBurstExplosionCommand()
    {
        Suite(nameof(VerifyPowampBurstExplosion), VerifyPowampBurstExplosion);
        return 0;
    }
}
