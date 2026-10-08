using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: Puromi's init ($A6:957E), sweep, projectile and sprite-object updates all place
    // the arc with EightBitNegativeSineMultiplication ($A0:B0C6). Using the positive sine
    // mirrored every Y offset about the origin: in the 100% movie the Pillar room's body
    // projectiles spawned at Y $B8 instead of native's $E8.
    private static void VerifyPuromiArcPosition()
    {
        const ushort nativeBodyY = 0x00e8;
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.LowerNorfairPillar);

        RoomEnemyProjectileSlot[] bodies = runtime.Enemies.EnemyProjectiles
            .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.NuclearWaffleBody)
            .ToArray();
        AssertEqual(8, bodies.Length, "the Pillar room spawns one Puromi's eight body projectiles");
        foreach (RoomEnemyProjectileSlot body in bodies)
            AssertEqual(nativeBodyY, body.YPosition, "Puromi body projectile spawn Y");
        Console.WriteLine("Puromi arc position: the arc uses the negative eight-bit sine, matching native spawn Y.");
    }
}
