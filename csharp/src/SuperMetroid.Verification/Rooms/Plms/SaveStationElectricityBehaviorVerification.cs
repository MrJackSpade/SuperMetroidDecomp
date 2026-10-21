using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.SaveStation;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>#1296: the save station's electricity runs its typed twenty-cycle program.</summary>
internal static class SaveStationElectricityBehaviorVerification
{
    /// <summary>The electricity's spawn starts its typed program, which cycles twenty times and deletes on the native update.</summary>
    internal static void VerifySaveStationElectricityBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, SaveStationElectricityNativeListAddresses.Extent, "save-station electricity");
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        enemies.Projectiles.SpawnSaveStationElectricity(42, 16);
        RoomEnemyProjectileSlot electricity = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        AssertTrue(electricity.Animation is SaveStationElectricityAnimation { Cursor.Program: SaveStationElectricityProgram.Start }, "the electricity starts its cycles");
        (bool deleted, int updates) = NativeEnemyProjectileParity.Compare(cartridge, enemies, electricity,
            SaveStationElectricityNativeListAddresses.Entry(SaveStationElectricityProgram.Start), 200, "save-station electricity");
        AssertTrue(deleted, "the electricity deletes after its cycles");
        AssertEqual(161, updates, "twenty eight-pose cycles, then the deletion update");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the electricity's native list bytes");
        Console.WriteLine("  Save-station electricity behavior: twenty eight-pose cycles match the native list.");
    }

    /// <summary>Runs the save-station electricity's program oracle and behavior suite.</summary>
    [VerificationCommand("--save-station-electricity-programs")]
    internal static int SaveStationElectricityProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(SaveStationElectricityAnimationsVerification.VerifySaveStationElectricityAnimations), () => SaveStationElectricityAnimationsVerification.VerifySaveStationElectricityAnimations(rom));
        Suite(nameof(VerifySaveStationElectricityBehavior), () => VerifySaveStationElectricityBehavior(rom));
        return 0;
    }
}
