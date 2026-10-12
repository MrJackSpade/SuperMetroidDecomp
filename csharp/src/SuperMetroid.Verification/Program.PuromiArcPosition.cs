using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class PuromiArcPositionVerification
{
    // #1269: Puromi's init ($A6:957E), sweep, projectile and sprite-object updates all place
    // the arc with EightBitNegativeSineMultiplication ($A0:B0C6). Using the positive sine
    // mirrored every Y offset about the origin: in the 100% movie the Pillar room's body
    // projectiles spawned at Y $B8 instead of native's $E8.
    internal static void VerifyPuromiArcPosition()
    {
        const ushort nativeBodyY = 0x00e8;
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.LowerNorfairPillar);

        RoomEnemyProjectileSlot[] bodies = runtime.Enemies.Projectiles.EnemyProjectiles
            .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.NuclearWaffleBody)
            .ToArray();
        AssertEqual(8, bodies.Length, "the Pillar room spawns one Puromi's eight body projectiles");
        foreach (RoomEnemyProjectileSlot body in bodies)
            AssertEqual(nativeBodyY, body.Position.Y.Whole, "Puromi body projectile spawn Y");
        Console.WriteLine("Puromi arc position: the arc uses the negative eight-bit sine, matching native spawn Y.");
    }

    [VerificationCommand("--puromi-arc-position")]
    internal static int PuromiArcPositionCommand()
    {
        VerifyPuromiArcPosition();
        return 0;
    }
}
