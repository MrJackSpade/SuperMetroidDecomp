using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Drawing;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Confirms an enemy's empty extended frame cannot receive a beam hit before its next solid frame.</summary>
internal static class EmptyExtendedFrameShotsVerification
{
    /// <summary>
    /// <c>$A0:9BA6</c> returns from the extended projectile pass while an enemy shows the
    /// empty extended frame <c>$804F</c>, before reading its point hitbox. In the 13% movie
    /// a charged beam passes over a green walking pirate on that frame without damaging it;
    /// the next frame's real body takes the hit.
    /// </summary>
    internal static void VerifyEmptyExtendedFrameShots()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xa521);
        RoomEnemySlot pirate = runtime.Enemies.SlotTable.Slots.First(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.PirateGreenWalking);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = true;

        // Native update 37306: the pirate at ($1F4,$A0) on $804F, the charged beam at ($1F2,$A3).
        pirate.Position = pirate.Position.WithWhole(new(0x01f4, 0x00a0));
        pirate.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.Frame;
        pirate.InstructionTimer = NativeCountdown.Start(0x7fff);
        int health = pirate.Health;
        runtime.Enemies.ProcessingWindow.PrepareEnemyProcessingList(pirate.Position.Whole);

        SamusProjectileSlot shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x9010; shot.Damage = 60; shot.Direction = 7;
        shot.Position = shot.Position.WithWhole(new(0x01f2, 0x00a3));
        shot.Radius = new(8, 8);
        shot.Cursor = ProjectileFixtureCursors.Active; shot.InstructionTimer = NativeCountdown.Start(1);
        int hits = runtime.Enemies.Collision.ResolveOrdinaryProjectileHits(runtime.Projectiles, runtime.BombProjectiles, samus,
            onlyNativeEnemyIndex: pirate.NativeIndex);

        AssertEqual(0, hits, "the empty extended frame takes no projectile hit");
        AssertEqual(health, pirate.Health, "the beam over the empty frame deals no damage");
        Console.WriteLine("  Empty extended frame: a beam over $804F leaves the pirate untouched.");
    }

    /// <summary>Runs the focused empty-frame projectile collision check.</summary>
    [VerificationCommand("--empty-extended-frame-shots")]
    internal static int EmptyExtendedFrameShotsCommand()
    {
        Suite(nameof(VerifyEmptyExtendedFrameShots), () => VerifyEmptyExtendedFrameShots());
        return 0;
    }
}
