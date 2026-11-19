using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn holds his stalk's links by reference (#1932): they are the projectiles the original reached at its fixed
/// places, and his death clears every other projectile while they survive.
/// </summary>
internal static class SporeSpawnStalkVerification
{
    /// <summary>
    /// Initializes Spore Spawn in a fresh room: his four links are the projectiles in places 14 through 17, from the ceiling
    /// down, where the original reached them, the ceiling link stands at the ceiling, and the death's clear deletes the
    /// spawners and a spore but none of the links, as the original's clear of places 0 through 13 did.
    /// </summary>
    internal static void VerifySporeSpawnStalkLinks()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().IsAreaMiniBossDefeated = () => false;
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.SporeSpawn, body.Definition);
        body.Position = body.Position.WithWhole(new(128, 624));
        body.Health = 960;
        enemies.SporeSpawnBehavior.InitializeSporeSpawn(body);
        SporeSpawnEnemyState state = body.RequireSpeciesState<SporeSpawnEnemyState>();

        AssertEqual(4, state.StalkLinks.Count, "Spore Spawn's stalk has four links");
        for (int link = 0; link < state.StalkLinks.Count; link++)
        {
            AssertTrue(ReferenceEquals(enemies.Projectiles.InPlace(14 + link), state.StalkLinks[link]),
                $"Spore Spawn's stalk link {link} is the projectile in the original's place {14 + link}");
        }
        AssertEqual(new Position16<RoomPixels>(128, 560), state.StalkLinks[0].Position.Whole,
            "Spore Spawn's first stalk link hangs at the ceiling");

        enemies.SporeSpawnBehavior.Spawner.SpawnSporeSpawnSpore(new(128, 600), HorizontalDirection.Right, state);
        AssertEqual(9, enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive),
            "the stalk, four spawners and a spore are live before the clear");
        enemies.SporeSpawnBehavior.DeleteProjectilesButStalk(state);
        EnemyProjectile[] live = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.IsActive)];
        AssertEqual(4, live.Length, "Spore Spawn's death clear leaves only his stalk");
        AssertTrue(live.All(projectile => state.StalkLinks.Contains(projectile)), "every survivor of the clear is a stalk link");
        AssertTrue(enemies.Projectiles.EnemyProjectiles.Take(14).All(projectile => !projectile.IsActive),
            "the clear empties places 0 through 13, as the original's did");
        Console.WriteLine("  Spore Spawn stalk: the links are the original's places 14 through 17 and survive his death's clear.");
    }

    /// <summary>Runs the Spore Spawn stalk-link check.</summary>
    [VerificationCommand("--spore-spawn-stalk")]
    internal static int SporeSpawnStalkCommand()
    {
        Suite(nameof(VerifySporeSpawnStalkLinks), () => VerifySporeSpawnStalkLinks());
        return 0;
    }
}
