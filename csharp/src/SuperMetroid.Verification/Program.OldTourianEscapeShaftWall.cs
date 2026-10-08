using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // #1269: during the escape the Climb's state runs room setup $8F:91A9, which spawns PLM
    // $84:B964. Its pre-instruction ($84:B927) waits for Samus to be strictly below and right
    // of ($F0,$820), then spawns explosion $86:B4B1 at the wall's middle ($110,$888) and
    // clears the two wall columns. The port never ran that setup, so in the 100% movie the
    // explosion native spawns into slot 15 never appeared.
    private static void VerifyOldTourianEscapeShaftWall()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesTimebombSet);
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.Climb);
        AssertTrue(runtime.Plms!.HasActiveHeader(RoomPlmHeaders.OldTourianEscapeShaftFakeWall),
            "the escape state's setup spawns the fake-wall PLM");

        int Explosions() => runtime.Enemies.EnemyProjectiles.Count(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion);
        SamusState samus = runtime.Samus!;
        samus.XPosition = 0x00e8;
        samus.YPosition = 0x088b;
        runtime.StepFrame(0);
        AssertEqual(0, Explosions(), "Samus left of X $F0 leaves the wall asleep");

        samus.XPosition = 0x00f1;
        samus.YPosition = 0x088b;
        runtime.StepFrame(0);
        AssertEqual(1, Explosions(), "Samus below and right of ($F0,$820) blows the wall");
        RoomEnemyProjectileSlot explosion = runtime.Enemies.EnemyProjectiles.First(projectile =>
            projectile.IsActive && projectile.Kind == RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion);
        AssertEqual((ushort)0x0110, explosion.Variable0, "the explosion is centred on the wall's X");
        AssertEqual((ushort)0x0888, explosion.Variable1, "the explosion is centred on the wall's Y");
        RoomLevelData level = runtime.LevelData!;
        for (int column = 0; column < 2; column++)
        for (int row = 0; row < 3; row++)
        {
            ushort word = level.GetCollisionBlockByIndex(level.GetBlockIndex(0x10 + column, 0x87 + row)).LevelWord;
            AssertEqual((ushort)0x00ff, word, $"wall block ({column},{row}) opens to $00FF");
        }
        Console.WriteLine("Old Tourian escape shaft wall: setup spawns the PLM; Samus past the target blows the wall.");
    }
}
