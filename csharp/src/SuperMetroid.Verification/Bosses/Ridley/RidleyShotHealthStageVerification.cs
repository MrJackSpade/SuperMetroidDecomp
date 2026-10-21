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
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Control.LockHandlers();
        RoomEnemySlot ridley = runtime.Enemies.SlotTable.Slots[0];
        AssertEqual(EnemyDefinitionId.Ridley, ridley.EnemyDefinitionPointer, "slot zero is Norfair Ridley");
        var state = runtime.Enemies.RidleyEncounter.RequireNorfairRidley(ridley);

        // Native update 56046: Ridley on frame $E983, both Super Missile slots on his body.
        ridley.Position = ridley.Position.WithWhole(new(0x00a4, 0x0148));
        ridley.SpritemapPointer = 0xe983;
        ridley.Properties = EnemyPropertyWord.FromNativeWord(0x3800);
        ridley.InvincibilityTimer = WrappingCountdown.Start(0);
        ridley.InstructionTimer = WrappingCountdown.Start(0x7fff);
        ridley.Health = 9340;
        state.FightMode = RidleyFightMode.PowerBombDodge;
        state.HealthStage = 0;
        runtime.Enemies.ProcessingWindow.PrepareEnemyProcessingList(ridley.Position.Whole);
        foreach (int index in new[] { 0, 1 })
        {
            SamusProjectileSlot shot = runtime.Projectiles.Slots[index];
            shot.Type = 0x8200; shot.Damage = 300; shot.Direction = 2;
            shot.Position = shot.Position.WithWhole(new(0x00c5, 0x0164));
            shot.Radius = new(8, 8);
            shot.Cursor = ProjectileFixtureCursors.Active; shot.InstructionTimer = WrappingCountdown.Start(1);
        }
        runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(runtime.Projectiles, runtime.BombProjectiles, samus,
            onlyNativeEnemyIndex: ridley.NativeIndex);

        AssertTrue(ridley.Health < 9000, "the hits cross 9000");
        AssertEqual(0, state.HealthStage, "the shot leaves the health stage for his AI to update");
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
