using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>#1291: the Work Robot's lasers run their typed prefix and loop as their native lists did.</summary>
internal static class WorkRobotLaserBehaviorVerification
{
    /// <summary>All five laser definitions start the typed prefix and run it and the loop update for update beside the native list.</summary>
    internal static void VerifyWorkRobotLaserBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, WorkRobotLaserNativeListAddresses.Extent, "Work Robot laser");
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        var robot = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112)),
            Radius = new(12, 16),
            VramTilesIndex = 512,
            PaletteIndex = 3072,
        };
        var state = new WorkRobotEnemyState();
        foreach ((WorkRobotLaser definition, short xVelocity) in new (WorkRobotLaser, short)[]
                 {
                     (WorkRobotLaser.UpLeft, -512), (WorkRobotLaser.Horizontal, -512), (WorkRobotLaser.DownLeft, -512),
                     (WorkRobotLaser.UpRight, 512), (WorkRobotLaser.DownRight, 512),
                 })
        {
            state.LaserVelocity = new(xVelocity, 0);
            enemies.WorkRobotBehavior.SpawnWorkRobotLaser(robot, state, definition, Position16<RoomPixels>.Zero);
        }
        RoomEnemyProjectileSlot[] lasers = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.Animation is WorkRobotLaserAnimation)];
        AssertEqual(5, lasers.Length, "all five laser definitions start the typed program");
        foreach (RoomEnemyProjectileSlot laser in lasers)
        {
            AssertTrue(laser.Animation is WorkRobotLaserAnimation { Cursor.Program: WorkRobotLaserProgram.Initial }, $"the {laser.Kind} laser starts its prefix");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, laser,
                NativeEnemyProjectileProgramOracle.EntryOf(WorkRobotLaserNativeListAddresses.Entries, WorkRobotLaserProgram.Initial), 80, laser.Kind.ToString());
            AssertTrue(!deleted, $"the {laser.Kind} laser loops until it is shot or collides");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the laser's native list bytes");
        Console.WriteLine("  Work Robot laser behavior: all five definitions run the prefix and loop as their native list does.");
    }

    /// <summary>Runs the Work Robot laser's program oracle and behavior suite.</summary>
    [VerificationCommand("--work-robot-laser-programs")]
    internal static int WorkRobotLaserProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(WorkRobotLaserAnimationsVerification.VerifyWorkRobotLaserAnimations), () => WorkRobotLaserAnimationsVerification.VerifyWorkRobotLaserAnimations(rom));
        Suite(nameof(VerifyWorkRobotLaserBehavior), () => VerifyWorkRobotLaserBehavior(rom));
        return 0;
    }
}
