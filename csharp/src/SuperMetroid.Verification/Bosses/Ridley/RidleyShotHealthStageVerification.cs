using SuperMetroid.Verification.Samus.Projectiles;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Bosses.Ridley;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

/// <summary>Reproduces a Ridley hit crossing the 9000-health threshold while
/// his shot reaction leaves acceleration stage changes for the later AI phase.</summary>
internal static class RidleyShotHealthStageVerification
{
    /// <summary>
    /// Ridley's shot reaction ($A6:DF8A) is the common no-death-check damage only. His health
    /// stage ($7E:7820 accelerationIndex) changes in the health-palette handler ($A6:D474) that
    /// his main and hurt AI run after moving, so the frame a hit crosses 9000 still accelerates
    /// with the old stage. In the 13% movie a Super Missile and its link take him from 9340 to
    /// 8740 while he dodges a power bomb.
    /// </summary>
    internal static void VerifyRidleyShotHealthStage()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Ridley);
        SamusState samus = runtime.Player.Samus;
        samus.Control.LockOutPlayer();
        RoomEnemyPlace ridley = runtime.Enemies.Places.InUpdateOrder[0];
        AssertEqual(EnemyDefinitionId.Ridley, ridley.Occupant?.DefinitionId, "slot zero is Norfair Ridley");
        var state = ridley.RequireSpeciesState<NorfairRidleyState>();

        // Native update 56046: Ridley on frame $E983, both Super Missile slots on his body.
        ridley.Position = ridley.Position.WithWhole(new(0x00a4, 0x0148));
        ridley.SpritemapPointer = 0xe983;
        ridley.Contact = EnemyContact.BlocksPlasmaBeam;
        ridley.Processing = EnemyProcessing.OffScreen | EnemyProcessing.Animates;
        ridley.Presence = EnemyPresence.None;
        ridley.Invincibility.End();
        ridley.AnimationClock.HoldFor(0x7fff);
        ridley.Health = 9340;
        state.FightMode = RidleyFightMode.PowerBombDodge;
        state.HealthBand = NorfairRidleyHealthBandDefinitions.Unhurt;
        runtime.Enemies.PrepareEnemyProcessingList(ridley.Position.Whole);
        foreach (int index in new[] { 0, 1 })
        {
            SamusProjectile shot = runtime.Projectiles.Shots[index];
            shot.Occupy(SamusProjectileType.Of(SamusProjectileFamily.SuperMissile), 300); shot.Direction = SamusProjectileDirection.Right;
            shot.Position = shot.Position.WithWhole(new(0x00c5, 0x0164));
            shot.Radius = new(8, 8);
            shot.Animation = ProjectileFixturePlaybacks.Active();
        }
        runtime.Enemies.Collision.ResolveOrdinaryProjectileHit(runtime.Projectiles, runtime.BombProjectiles, runtime.Room.LoadedRoom.Area, samus,
            ridley.HeldOccupant(), Position16<RoomPixels>.Zero);

        AssertTrue(ridley.Health < 9000, "the hits cross 9000");
        AssertTrue(ReferenceEquals(NorfairRidleyHealthBandDefinitions.Unhurt, state.HealthBand), "the shot leaves the health band for his AI to update");
        Console.WriteLine("  Ridley shot health stage: a hit crossing 9000 leaves the stage to the AI's palette handler.");
    }

    /// <summary>Runs the focused Ridley shot and health-stage reproduction from the CLI.</summary>
    [VerificationCommand("--ridley-shot-health-stage")]
    internal static int RidleyShotHealthStageCommand()
    {
        Suite(nameof(VerifyRidleyShotHealthStage), () => VerifyRidleyShotHealthStage());
        return 0;
    }
}
