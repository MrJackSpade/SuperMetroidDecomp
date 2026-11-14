using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Enemies.SaveStation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>#1296: the save station's electricity runs its typed twenty-cycle program.</summary>
internal static class SaveStationElectricityBehaviorVerification
{
    /// <summary>The electricity's spawn starts its typed program, which cycles twenty times and deletes on the native update.</summary>
    internal static void VerifySaveStationElectricityBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, SaveStationElectricityNativeListAddresses.Extent, "save-station electricity");
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        enemies.SaveStationElectricity.Spawn(new(10, 2));
        EnemyProjectile electricity = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        AssertTrue(electricity.Animation is SaveStationElectricityAnimation { PlayingSegment: SaveStationElectricitySegment.Start }, "the electricity starts its cycles");
        (bool deleted, int updates) = NativeEnemyProjectileParity.Compare(cartridge, enemies, electricity,
            SaveStationElectricityNativeListAddresses.Entry(SaveStationElectricitySegment.Start), 200, "save-station electricity");
        AssertTrue(deleted, "the electricity deletes after its cycles");
        AssertEqual(161, updates, "twenty eight-pose cycles, then the deletion update");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the electricity's native list bytes");
        Console.WriteLine("  Save-station electricity behavior: twenty eight-pose cycles match the native list.");
    }

    /// <summary>Runs the save-station electricity's program oracle and behavior suite.</summary>
    [VerificationCommand("--save-station-electricity-programs")]
    internal static int SaveStationElectricityProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(SaveStationElectricityAnimationsVerification.VerifySaveStationElectricityAnimations), () => SaveStationElectricityAnimationsVerification.VerifySaveStationElectricityAnimations(rom));
        Suite(nameof(VerifySaveStationElectricityBehavior), () => VerifySaveStationElectricityBehavior(rom));
        return 0;
    }
}
