using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

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

        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        EnemyProjectile[] pool = [.. enemies.Projectiles.EnemyProjectiles];
        // Occupy every place above the ninth and the first, so the eight spikes take the second through ninth places.
        foreach (EnemyProjectile occupied in pool.Where(slot => slot.Place is 0 or > 8))
            occupied.Kind = RoomEnemyProjectileKind.MiscDustExplosion;
        var body = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
        };
        AssertEqual<int?>(1, enemies.PowampBehavior.SpawnPowampSpikeBurst(body, body.Shading), "the burst's last spike takes the second place");
        // Each spike is a new projectile in its place, so the places are read again after the burst (#1590).
        AssertEqual(8, enemies.Projectiles.EnemyProjectiles.Count(slot => slot.Kind == RoomEnemyProjectileKind.PowampSpike), "the burst fires all eight spikes");

        foreach (EnemyProjectile slot in enemies.Projectiles.EnemyProjectiles)
            slot.Delete();
        foreach (EnemyProjectile occupied in enemies.Projectiles.EnemyProjectiles.Where(slot => slot.Place > 4))
            occupied.Kind = RoomEnemyProjectileKind.MiscDustExplosion;
        AssertEqual<int?>(null, enemies.PowampBehavior.SpawnPowampSpikeBurst(body, body.Shading), "a burst finding the pool full before its eighth spike reports none");
        Console.WriteLine("  Powamp burst explosion: the last spike's pool place selects the explosion; a full pool selects the small one.");
    }

    /// <summary>The explosion a Powamp dies with after a burst whose last spike took <paramref name="place"/>.</summary>
    private static EnemyDeathExplosion ExplosionAfter(int? place) =>
        PowampBurstExplosion.AfterBurst(place, EnemyDeathExplosionDefinitions.AuthoredOrder);

    /// <summary>Runs the Powamp burst explosion rule check.</summary>
    [VerificationCommand("--powamp-burst-explosion")]
    internal static int PowampBurstExplosionCommand()
    {
        Suite(nameof(VerifyPowampBurstExplosion), VerifyPowampBurstExplosion);
        return 0;
    }
}
