using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that a flying Draygon goop reaches its touch list in native processing order.</summary>
internal static class DraygonGoopTouchOrderVerification
{
    /// <summary>
    /// `$86:8E0F` moves the goop and tests Samus before the room-boundary deletion, and `$86:8125`
    /// runs the instruction list after the pre-instruction without re-reading the ID. A goop a power
    /// bomb releases while it reaches Samus therefore still runs its touch list (`$86:8D99`): it
    /// attaches and clears her invincibility and knockback. The contributed lowc movie shows it in
    /// Draygon's room: the cleared invincibility lets Draygon's next contact hurt Samus again.
    /// </summary>
    internal static void VerifyDraygonGoopTouchOrder()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach ((bool powerBombed, ushort goopX, ushort samusX) in new[] { (true, (ushort)170, (ushort)178), (false, (ushort)514, (ushort)504) })
        {
            var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Draygon);
            SamusState samus = runtime.Player.IntroducedSamus;
            samus.Position = FixedPosition<RoomPixels>.FromWhole(new(samusX, 385));
            samus.InvincibilityTimer = NativeCountdown.Start(96);
            samus.KnockbackTimer = NativeCountdown.Start(5);
            samus.XSpeedDivisor = 0;
            var enemies = runtime.Enemies;
            var goop = enemies.Projectiles.EnemyProjectiles[^1];
            goop.Kind = RoomEnemyProjectileKind.DraygonGoop;
            goop.PreInstruction = EnemyProjectilePreInstruction.DraygonGoop;
            goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileProgram.Goop));
            goop.Position = FixedPosition<RoomPixels>.FromWhole(new(goopX, 391));
            var powerBomb = runtime.BombProjectiles.PowerBombExplosion;
            if (powerBombed)
            {
                powerBomb.Spawn(goop.Position.Whole);
                powerBomb.ExplosionRadius = PowerBombRadius.FromRaw(8192); // 32 pixels in 8.8
            }
            enemies.Audio.PowerBomb = powerBomb;

            enemies.StepEnemyProjectiles(runtime.Room.LoadedLevelData, samus);

            string context = powerBombed ? "power-bombed goop at Samus" : "goop at Samus beyond the room edge";
            AssertEqual((ushort)1, samus.XSpeedDivisor, $"{context} attaches through its touch list");
            AssertTrue(samus.InvincibilityTimer.IsZero && samus.KnockbackTimer.IsZero,
                $"{context} clears Samus's invincibility and knockback");
            AssertEqual(!powerBombed, goop.IsActive, $"{context} {(powerBombed ? "stays released" : "stays attached")}");
        }
        Console.WriteLine("Draygon goop touch order: contact precedes the room boundary and survives a power-bomb release.");
    }

    /// <summary>Runs the Draygon goop touch-order check from the verification command line.</summary>
    [VerificationCommand("--draygon-goop-touch-order")]
    internal static int DraygonGoopTouchOrderCommand()
    {
        Suite(nameof(VerifyDraygonGoopTouchOrder), () => VerifyDraygonGoopTouchOrder());
        return 0;
    }
}
