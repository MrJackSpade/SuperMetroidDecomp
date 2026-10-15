using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the old Tourian escape shaft wall PLM's trigger, explosion, and cleared blocks.</summary>
internal static class OldTourianEscapeShaftWallVerification
{
    /// <summary>Confirms escape setup $8F:91A9 spawns the wall PLM, which opens two columns after Samus crosses its strict position gate.</summary>
    /// <remarks>#1269: pre-instruction $84:B927 spawns explosion $86:B4B1 at ($110,$888). The missing setup had omitted that explosion in the 100% movie.</remarks>
    internal static void VerifyOldTourianEscapeShaftWall()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.Climb);
        AssertTrue(runtime.Plms.BombTorizoHand.HasActiveHeader(PlmHeaderId.OldTourianEscapeShaftFakeWall),
            "the escape state's setup spawns the fake-wall PLM");

        int Explosions() => runtime.Enemies.Projectiles.EnemyProjectiles.Count(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Position = samus.Position.WithWhole(new(0x00e8, 0x088b));
        runtime.StepFrame(0);
        AssertEqual(0, Explosions(), "Samus left of X $F0 leaves the wall asleep");

        samus.Position = samus.Position.WithWhole(new(0x00f1, 0x088b));
        runtime.StepFrame(0);
        AssertEqual(1, Explosions(), "Samus below and right of ($F0,$820) blows the wall");
        RoomEnemyProjectileSlot explosion = runtime.Enemies.Projectiles.EnemyProjectiles.First(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion);
        AssertEqual((ushort)0x0110, explosion.Variable0, "the explosion is centred on the wall's X");
        AssertEqual((ushort)0x0888, explosion.Variable1, "the explosion is centred on the wall's Y");
        RoomLevelData level = runtime.Room.LoadedLevelData;
        for (int column = 0; column < 2; column++)
        for (int row = 0; row < 3; row++)
        {
            ushort word = level.GetCollisionBlockByIndex(level.GetBlockIndex(new(checked((ushort)(0x10 + column)), checked((ushort)(0x87 + row))))).LevelWord;
            AssertEqual((ushort)0x00ff, word, $"wall block ({column},{row}) opens to $00FF");
        }
        Console.WriteLine("Old Tourian escape shaft wall: setup spawns the PLM; Samus past the target blows the wall.");
    }

    /// <summary>Runs the focused old Tourian escape shaft wall verification command.</summary>
    [VerificationCommand("--old-tourian-escape-shaft-wall")]
    internal static int OldTourianEscapeShaftWallCommand()
    {
        VerifyOldTourianEscapeShaftWall();
        return 0;
    }
}
