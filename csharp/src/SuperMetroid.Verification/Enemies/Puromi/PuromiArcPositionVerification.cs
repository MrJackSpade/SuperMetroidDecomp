using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Enemies.Puromi;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Puromi's negative-sine body arc in the Lower Norfair Pillar room.</summary>
internal static class PuromiArcPositionVerification
{
    /// <summary>Confirms all eight body projectiles spawn at native Y $E8 using the negative-sine arc.</summary>
    internal static void VerifyPuromiArcPosition()
    {
        const ushort nativeBodyY = 0x00e8;
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture();
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

    /// <summary>Runs the focused Puromi arc-position check.</summary>
    [VerificationCommand("--puromi-arc-position")]
    internal static int PuromiArcPositionCommand()
    {
        VerifyPuromiArcPosition();
        return 0;
    }
}
