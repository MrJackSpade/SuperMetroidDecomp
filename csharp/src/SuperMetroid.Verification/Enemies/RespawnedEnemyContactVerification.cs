using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Grapple;
namespace SuperMetroid.Verification.Enemies;

using SuperMetroid.Verification.Harness;

/// <summary>Reproduces the Hellway Zebbo respawn's delayed contact and grapple eligibility.</summary>
internal static class RespawnedEnemyContactVerification
{
    /// <summary>
    /// Confirms Respawn_Enemy ($86:F264) leaves the spritemap cleared until the first
    /// instruction; $A0:A08C skips Samus contact while it is zero. The 100% movie's
    /// Zebbo becomes eligible on the next frame, after interactive-list rebuilding.
    /// </summary>
    internal static void VerifyRespawnedEnemyContact()
    {
        const EnemyDefinitionId respawningEnemy = EnemyDefinitionId.Zebbo;
        const ushort respawnX = 0x1e0;
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Hellway);
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();

        RoomEnemySlot enemy = runtime.Enemies.SlotTable.Slots.Single(slot =>
            slot.EnemyDefinitionPointer == respawningEnemy && slot.Position.X.Whole == respawnX);
        AssertTrue(enemy.Properties.HasAny(EnemyProperties.RespawnIfKilled),
            "the Hellway actor respawns when killed");
        runtime.Enemies.DeathsAndDrops.StartGenericEnemyDeath(enemy, deathAnimation: 0);

        StepUntil(
            () => enemy.EnemyDefinitionPointer == respawningEnemy,
            _ => runtime.StepFrame(0),
            maximumFrames: 200,
            context: "Hellway enemy respawn");
        AssertEqual((ushort)0, enemy.SpritemapPointer,
            "Respawn_Enemy does not install the empty spritemap");

        // #1269: game state eight rebuilds the interactive list ($A0:8EB6) before Samus's
        // grapple runs. The list built during the respawn frame still excluded the $DAFF
        // placeholder; only the next frame's prologue admits the respawned actor.
        AssertTrue(runtime.Enemies.GrappleBehavior.ResolveGrappleEndpoint(enemy.Position.Whole).Reaction == GrappleEnemyReaction.None,
            "the respawn frame's interactive list does not yet contain the respawned actor");
        runtime.Enemies.ProcessingWindow.PrepareEnemyProcessingList(new(runtime.Room.LoadedCamera.PixelPosition.X, runtime.Room.LoadedCamera.PixelPosition.Y));
        AssertTrue(runtime.Enemies.GrappleBehavior.ResolveGrappleEndpoint(enemy.Position.Whole).Reaction != GrappleEnemyReaction.None,
            "the next frame's prologue list lets the grapple reach the respawned actor");
        enemy.AiStates = EnemyAiStates.None;
        // $A0:A08C rejects Samus contact while this word is zero; the first instruction
        // then installs the actor's map and contact can resume on the following pass.
        runtime.StepFrame(0);
        AssertEqual((ushort)0x8a82, enemy.SpritemapPointer,
            "the respawned actor's first instruction installs its first spritemap");
        Console.WriteLine("Respawned enemy contact: Respawn_Enemy keeps a zero spritemap until the first instruction.");
    }

    /// <summary>Runs the focused Hellway respawn visibility and contact check.</summary>
    [VerificationCommand("--respawned-enemy-contact")]
    internal static int RespawnedEnemyContactCommand()
    {
        VerifyRespawnedEnemyContact();
        return 0;
    }
}
