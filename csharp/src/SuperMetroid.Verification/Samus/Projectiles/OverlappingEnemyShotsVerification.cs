using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;

/// <summary>Checks that a missile hits all overlapping Beetoms before deferred projectile removal.</summary>
internal static class OverlappingEnemyShotsVerification
{
    /// <summary>Runs the real Beetom hit callback against a three-enemy stack, then confirms the next projectile pass removes the spent shot.</summary>
    internal static void VerifyOverlappingEnemyShots()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = _ => { };
        var interactive = (List<RoomEnemyOccupant>)enemies.ProcessingWindow.InteractiveEnemies;
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(64, 128)) };
        // The room's population load sets up three Beetoms stacked on one another (#2554).
        _ = enemies.LoadPopulation(samus, [.. Enumerable.Repeat(PopulationPlacementAccess.Entry(EnemyDefinitionId.Beetom, new Position16<RoomPixels>(128, 128)), 3)]);
        for (int index = 0; index < 3; index++)
        {
            var enemy = enemies.Places.InUpdateOrder[index];
            // The Beetom was set up as its room's population sets it up, keeping its real callback and vulnerability, then
            // is given enough health to observe damage independently from the death actor and drop machinery.
            enemy.Radius = new(enemy.Definition.Radius.X, enemy.Definition.Radius.Y);
            enemy.Health = 180;
            enemy.Frame = EnemyFrameId.BeetomCrawlingLeftLegsTucked; // Shots reach only an enemy showing a frame.
            interactive.Add(enemy.HeldOccupant());
        }
        var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: InstalledFixtures.Projectiles.FrameBindings), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: InstalledFixtures.Projectiles.FrameBindings));
        var shot = shots.Shots[0];
        shot.Occupy(SamusProjectileType.Of(SamusProjectileFamily.Missile), 100);
        shot.HeldShot().Direction = SamusProjectileDirection.Right;
        shot.HeldShot().Position = shot.Position.WithWhole(new(128, 128));
        shot.HeldShot().Radius = new(4, 4);
        shot.HeldShot().Animation = ProjectileFixturePlaybacks.Active();
        shot.HeldShot().Install(SamusShotBehaviors.Missile, new MissileShotState(new SamusMissileIgnition.Pending()));

        enemies.ResolveShotsOnInteractiveEnemies(shots, bombs, SuperMetroid.Core.Progression.AreaId.Crateria, samus);
        foreach (var enemy in enemies.Places.InUpdateOrder.Take(3))
            AssertEqual(80, enemy.Health, "each overlapping Beetom takes native missile damage");
        AssertTrue(shot.IsSpent, "non-piercing shot is marked for deferred removal");
        AssertEqual(SamusProjectileType.Of(SamusProjectileFamily.Missile), shot.HeldShot().Type, "collision preserves the missile until projectile processing");
        var level = CreateRoom(new(16, 16), new RoomTerrainBlock[256], new RoomBlockBehavior[256]);
        shots.HandleShots(new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled }, samus, Position16<RoomPixels>.Zero, bombs, new SharedGameState(new SharedRandom()));
        AssertTrue(!shot.IsOccupied, "next projectile pass consumes the removal marker");
        foreach (var enemy in enemies.Places.InUpdateOrder.Take(3))
        {
            enemy.AiStates = EnemyAiStates.None;
            enemy.HurtFlash.End();
            enemy.Invincibility.End();
        }
        enemies.ResolveShotsOnInteractiveEnemies(shots, bombs, SuperMetroid.Core.Progression.AreaId.Crateria, samus);
        foreach (var enemy in enemies.Places.InUpdateOrder.Take(3))
            AssertEqual(80, enemy.Health, "removed projectile cannot damage the stack on the next pass");
        Console.WriteLine("Overlapping Beetoms: all three receive one shot's damage; deferred removal prevents a later hit.");
    }

    /// <summary>Runs the overlapping Beetom collision reproduction.</summary>
    [VerificationCommand("--overlapping-enemy-shots")]
    internal static int OverlappingEnemyShotsCommand()
    {
        Suite(nameof(VerifyOverlappingEnemyShots), () => VerifyOverlappingEnemyShots());
        return 0;
    }
}
