using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Enemies.Deaths;

namespace SuperMetroid.Verification.Enemies.Powamp;

/// <summary>
/// #1513: a Powamp shot dead explodes with the explosion its burst's last spike's place in the projectile pool selects,
/// twice that place names (<see cref="LeftoverNumberDeathExplosion{TExplosion}"/>), rather than a number read from its graphics.
/// </summary>
internal static class PowampBurstExplosionVerification
{
    /// <summary>
    /// The rule gives, in the explosions' authored order, the small explosion for the first place, the normal one for the
    /// second, the big one for the third and the small one for any later place or a full pool. A Powamp shot dead in a pool
    /// whose free places are its first through ninth fires its spikes into the ninth down to the second and dies in the
    /// normal explosion, in the first; one whose pool fills before its eighth spike leaves no place, nor any for its
    /// explosion.
    /// </summary>
    internal static void VerifyPowampBurstExplosion()
    {
        AssertEqual(EnemyDeathExplosion.Small, ExplosionAfter(0), "a last spike in the first place selects the small explosion");
        AssertEqual(EnemyDeathExplosion.Normal, ExplosionAfter(1), "a last spike in the second place selects the normal explosion");
        AssertEqual(EnemyDeathExplosion.Big, ExplosionAfter(2), "a last spike in the third place selects the big explosion");
        AssertEqual(EnemyDeathExplosion.Small, ExplosionAfter(3), "a last spike in the fourth place selects the small explosion");
        AssertEqual(EnemyDeathExplosion.Small, ExplosionAfter(17), "a last spike in the pool's last place selects the small explosion");
        AssertEqual(EnemyDeathExplosion.Small, ExplosionAfter(null), "a burst that found the pool full selects the small explosion");

<<<<<<< ours
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // Hold every place above the ninth and the first, so the eight spikes take the second through ninth places: spawns fill
        // the pool and those in the second through ninth places are deleted.
        foreach (EnemyProjectile spawned in FamilyProjectileStarts.FillPool(enemies, RoomEnemyProjectileKind.DustOrExplosion, SpriteGraphics.SharedStandard)
            .Where(spawned => spawned.Place is >= 1 and <= 8))
            spawned.Delete();
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112));
        enemies.PowampBehavior.SpawnPowampSpikeBurst(body.BoundPlaced(EnemyDefinitionId.Powamp, new PowampBodySpawnSettings(PowampContact.Accepts)), body, body.Shading);
        AssertEqual<int?>(1, enemies.Projectiles.Places.LatestSpawn, "the burst's last spike takes the second place");
        // Each spike is a new projectile in its place, so the places are read again after the burst (#1590).
        AssertEqual(8, enemies.Projectiles.EnemyProjectiles.Count(slot => slot.Kind == RoomEnemyProjectileKind.PowampSpike), "the burst fires all eight spikes");
||||||| base
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // Occupy every place above the ninth and the first, so the eight spikes take the second through ninth places.
        foreach (int place in Enumerable.Range(0, SuperMetroid.Core.Compatibility.EnemyProjectilePlaces.Capacity).Where(place => place is 0 or > 8))
            enemies.Projectiles.Occupy(place, RoomEnemyProjectileKind.DustOrExplosion);
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112));
        enemies.PowampBehavior.SpawnPowampSpikeBurst(body.BoundPlaced(EnemyDefinitionId.Powamp, new PowampBodySpawnSettings(PowampContact.Accepts)), body, body.Shading);
        AssertEqual<int?>(1, enemies.Projectiles.Places.LatestSpawn, "the burst's last spike takes the second place");
        // Each spike is a new projectile in its place, so the places are read again after the burst (#1590).
        AssertEqual(8, enemies.Projectiles.EnemyProjectiles.Count(slot => slot.Kind == RoomEnemyProjectileKind.PowampSpike), "the burst fires all eight spikes");
=======
        // Every place above the ninth is taken, so the eight spikes take the second through ninth places and the
        // explosion the first. Each spike is a new projectile in its place, so the places are read after the death (#1590).
        RoomEnemySystem enemies = ShotDeadWithPlacesTaken(place => place > 8);
        EnemyProjectile[] spikes = [.. enemies.Projectiles.EnemyProjectiles.Where(slot => slot.Kind == RoomEnemyProjectileKind.PowampSpike)];
        AssertEqual(8, spikes.Length, "the burst fires all eight spikes");
        AssertEqual(1, spikes.Min(spike => spike.Place), "the burst's last spike takes the second place");
        EnemyProjectile explosion = enemies.Projectiles.EnemyProjectiles.Single(slot => slot.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion);
        AssertEqual(0, explosion.Place, "the Powamp's explosion takes the first place");
        AssertEqual(EnemyDeathExplosions.Animation(EnemyDeathExplosion.Normal), EnemyDeathProjectilePrograms.AnimationOf(explosion),
            "a last spike in the second place makes the Powamp die in the normal explosion");
>>>>>>> theirs

<<<<<<< ours
        foreach (EnemyProjectile slot in enemies.Projectiles.EnemyProjectiles)
            slot.Delete();
        // Spawns searching from the last place down hold every place above the fifth.
        for (int held = 5; held < SuperMetroid.Core.Compatibility.EnemyProjectilePlaces.Capacity; held++)
            FamilyProjectileStarts.Allocate(enemies, RoomEnemyProjectileKind.DustOrExplosion, SpriteGraphics.SharedStandard);
        enemies.PowampBehavior.SpawnPowampSpikeBurst(body.HeldOccupant(), body, body.Shading);
||||||| base
        foreach (EnemyProjectile slot in enemies.Projectiles.EnemyProjectiles)
            slot.Delete();
        foreach (int place in Enumerable.Range(0, SuperMetroid.Core.Compatibility.EnemyProjectilePlaces.Capacity).Where(place => place > 4))
            enemies.Projectiles.Occupy(place, RoomEnemyProjectileKind.DustOrExplosion);
        enemies.PowampBehavior.SpawnPowampSpikeBurst(body.HeldOccupant(), body, body.Shading);
=======
        // Every place above the fifth is taken, so five spikes take the first through fifth places and the sixth finds the
        // pool full; so does the explosion.
        enemies = ShotDeadWithPlacesTaken(place => place > 4);
        AssertEqual(5, enemies.Projectiles.EnemyProjectiles.Count(slot => slot.Kind == RoomEnemyProjectileKind.PowampSpike),
            "the burst fires spikes until the pool is full");
>>>>>>> theirs
        AssertEqual<int?>(null, enemies.Projectiles.Places.LatestSpawn, "a burst finding the pool full before its eighth spike leaves no place");
        AssertTrue(!enemies.Projectiles.EnemyProjectiles.Any(slot => slot.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion),
            "the full pool has no place for the Powamp's explosion");
        Console.WriteLine("  Powamp burst explosion: the last spike's pool place selects the explosion; a full pool selects the small one.");
    }

    /// <summary>
    /// A Powamp shot dead in a room whose projectile places <paramref name="taken"/> selects are taken, its death delay run
    /// out: its spikes have burst and it has died in its explosion.
    /// </summary>
    private static RoomEnemySystem ShotDeadWithPlacesTaken(Func<int, bool> taken)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        foreach (int place in Enumerable.Range(0, SuperMetroid.Core.Compatibility.EnemyProjectilePlaces.Capacity).Where(taken))
            enemies.Projectiles.Occupy(place, RoomEnemyProjectileKind.DustOrExplosion);
        RoomEnemyPlace body = PowampPairFixtures.PlacePair(enemies).Body;
        body.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112));
        PowampPairFixtures.ShootDeadAndRunDeathDelay(enemies, body);
        return enemies;
    }

    /// <summary>
    /// The explosion a Powamp dies with after a burst whose last spike took <paramref name="place"/>, or that found the
    /// pool full: the one the pool's latest spawn leaves (<see cref="LeftoverNumberDeathExplosion{TExplosion}.LeftByLatestSpawn"/>).
    /// </summary>
    internal static EnemyDeathExplosion ExplosionAfter(int? place)
    {
        var places = new SuperMetroid.Core.Compatibility.EnemyProjectilePlaces();
        places.RecordSpawn(place);
        return EnemyDeathExplosionDefinitions.LeftoverNumbered.LeftByLatestSpawn(places);
    }

    /// <summary>Runs the Powamp burst explosion rule check.</summary>
    [VerificationCommand("--powamp-burst-explosion")]
    internal static int PowampBurstExplosionCommand()
    {
        Suite(nameof(VerifyPowampBurstExplosion), VerifyPowampBurstExplosion);
        return 0;
    }
}
