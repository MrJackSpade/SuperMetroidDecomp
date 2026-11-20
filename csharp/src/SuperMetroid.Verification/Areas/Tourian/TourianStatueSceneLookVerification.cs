using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>
/// Checks that the Tourian entrance statue scene shows its look on the placements it set up and the projectiles it
/// spawned, and on nothing else in the room (#2356): a placement that has left its place drops out of the scene (#2426).
/// </summary>
internal static class TourianStatueSceneLookVerification
{
    /// <summary>
    /// Sets up the scene's first placement, which spawns its three decorations, beside a Tourian statue place the scene
    /// never set up and a projectile that is not the scene's, then lights a statue's eyes: the placement, the decorations
    /// and the eye glow show the scene's look with the glow, and the other place and the other projectile keep their
    /// standard colours. Once the placement has left and another enemy holds its place, a later look leaves that enemy's
    /// colours as they were.
    /// </summary>
    internal static void VerifyTourianStatueSceneLook()
    {
        RoomEnemySystem enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles),
            RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: InstalledFixtures.BossArtwork,
            statueRoom: new FixedTourianStatueRoom(WaterSurface: 216, StatueOffset: 0));
        RoomEnemyPlace placement = enemies.Places.InUpdateOrder[0];
        placement.SetSpawnParameter1(EnemyDefinitionId.TourianStatue, 0);
        placement.Bind(EnemyDefinitionId.TourianStatue, placement.KeptDefinition);
        RoomEnemyPlace unplaced = enemies.Places.InUpdateOrder[1];
        unplaced.Bind(EnemyDefinitionId.TourianStatue, unplaced.KeptDefinition);

        EnemyProjectile stranger = enemies.Projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.DustOrExplosion, DustExplosionProjectileBehavior.Instance, SpriteGraphics.SharedStandard)
            ?? throw new InvalidOperationException("A new room has a free enemy-projectile place.");

        TourianEntranceStatueBehavior scene = EnemySystemFixtures.StatueScene(enemies);
        EnemySystemFixtures.InitializeStatuePlacement(placement);
        EnemyProjectile[] decorations = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile =>
            projectile.IsActive && !ReferenceEquals(projectile, stranger))];
        AssertEqual(3, decorations.Length, "the scene's first placement spawns its three decorations");

        EnemySystemFixtures.UnlockEffects(enemies).SpawnTourianEyeGlow(TourianStatueNativeWords.Eye(0));
        EnemyProjectile glow = enemies.Projectiles.EnemyProjectiles.Single(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.TourianStatueEyeGlow);
        PictureShading look = scene.SceneShading;
        AssertTrue(!look.IsStandard, "a lit eye gives the scene a look of its own");

        AssertEqual(look, placement.Shading, "the placement the scene set up shows the scene's look");
        foreach (EnemyProjectile decoration in decorations)
            AssertEqual(look, decoration.Graphics.Shading, $"the scene's {decoration.Kind} shows the scene's look");
        AssertEqual(look, glow.Graphics.Shading, "the eye glow the scene spawned shows the scene's look");
        AssertTrue(unplaced.Shading.IsStandard, "a statue place the scene never set up keeps its standard colours");
        AssertTrue(stranger.Graphics.Shading.IsStandard, "a projectile that is not the scene's keeps its standard colours");

        // The placement leaves, and an enemy of another kind takes its place: the scene's next look is not shown there.
        placement.Bind(EnemyDefinitionId.Zoa, placement.KeptDefinition);
        PictureShading newcomer = placement.Shading;
        EnemySystemFixtures.UnlockEffects(enemies).SpawnTourianEyeGlow(TourianStatueNativeWords.Eye(2));
        AssertTrue(scene.SceneShading != look, "a second lit eye gives the scene a further look");
        AssertEqual(newcomer, placement.Shading, "a place the scene's placement has left keeps its new occupant's colours");
        Console.WriteLine("  Tourian statue scene: its look shows on its own placement, decorations and eye glow, and on nothing else.");
    }

    /// <summary>Runs the statue scene look check.</summary>
    [VerificationCommand("--tourian-statue-scene-look")]
    internal static int TourianStatueSceneLookCommand()
    {
        Suite(nameof(VerifyTourianStatueSceneLook), () => VerifyTourianStatueSceneLook());
        return 0;
    }
}
