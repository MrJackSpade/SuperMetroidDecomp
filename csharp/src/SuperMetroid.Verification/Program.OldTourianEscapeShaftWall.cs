using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using static RetailRuntimeFixtureVerification;

internal static class OldTourianEscapeShaftWallVerification
{
    // #1269: during the escape the Climb's state runs room setup $8F:91A9, which spawns PLM
    // $84:B964. Its pre-instruction ($84:B927) waits for Samus to be strictly below and right
    // of ($F0,$820), then spawns explosion $86:B4B1 at the wall's middle ($110,$888) and
    // clears the two wall columns. The port never ran that setup, so in the 100% movie the
    // explosion native spawns into slot 15 never appeared.
    internal static void VerifyOldTourianEscapeShaftWall()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.Climb);
        AssertTrue(runtime.Plms!.BombTorizoHand.HasActiveHeader(PlmHeaderId.OldTourianEscapeShaftFakeWall),
            "the escape state's setup spawns the fake-wall PLM");

        int Explosions() => runtime.Enemies.Projectiles.EnemyProjectiles.Count(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion);
        SamusState samus = runtime.Player.Samus!;
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
        RoomLevelData level = runtime.Room.LevelData!;
        for (int column = 0; column < 2; column++)
        for (int row = 0; row < 3; row++)
        {
            ushort word = level.GetCollisionBlockByIndex(level.GetBlockIndex(new(checked((ushort)(0x10 + column)), checked((ushort)(0x87 + row))))).LevelWord;
            AssertEqual((ushort)0x00ff, word, $"wall block ({column},{row}) opens to $00FF");
        }
        Console.WriteLine("Old Tourian escape shaft wall: setup spawns the PLM; Samus past the target blows the wall.");
    }

    [VerificationCommand("--old-tourian-escape-shaft-wall")]
    internal static int OldTourianEscapeShaftWallCommand()
    {
        VerifyOldTourianEscapeShaftWall();
        return 0;
    }
}
