using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.FallingSparks;

/// <summary>#1291: a falling spark runs its typed flight and floor-impact programs as its native lists did.</summary>
internal static class FallingSparkBehaviorVerification
{
    /// <summary>
    /// A spawned spark starts its falling program and loops it update for update beside the native list; the floor
    /// impact starts the impact program, which blinks through its eleven frames and deletes the spark on the native update.
    /// </summary>
    internal static void VerifyFallingSparkBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, FallingSparkNativeListAddresses.Extent, "falling-spark");
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);

        RoomEnemySlot source = enemies.SlotTable.Slots[0];
        source.Position = source.Position.WithWhole(new(288, 96));
        enemies.FallingSparksBehavior.SpawnFallingSpark(source);
        RoomEnemyProjectileSlot spark = enemies.Projectiles.EnemyProjectiles.Single(candidate => candidate.Kind == RoomEnemyProjectileKind.FallingSpark);
        AssertEqual(FallingSparkProgram.Falling, ProgramOf(spark), "a spawned spark starts its falling program");
        (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, spark, Entry(FallingSparkProgram.Falling), 30, "falling spark flight");
        AssertTrue(!deleted, "the falling program loops until the floor");

        FallingSparksBehavior.BeginFallingSparkFloorImpact(spark);
        AssertEqual(FallingSparkProgram.HitFloor, ProgramOf(spark), "the floor impact starts the impact program");
        (deleted, int updates) = NativeEnemyProjectileParity.Compare(cartridge, enemies, spark, Entry(FallingSparkProgram.HitFloor), 30, "falling spark impact");
        AssertTrue(deleted, "the impact program deletes the spark");
        AssertEqual(12, updates, "the impact shows eleven one-update frames before deleting the spark");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the spark's native list bytes");
        Console.WriteLine("  Falling spark behavior: the falling loop and the eleven-frame impact match their native lists update for update.");
    }

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    private static ushort Entry(FallingSparkProgram program) =>
        NativeEnemyProjectileProgramOracle.EntryOf(FallingSparkNativeListAddresses.Entries, program);

    /// <summary>The typed program <paramref name="projectile"/> runs.</summary>
    private static FallingSparkProgram ProgramOf(RoomEnemyProjectileSlot projectile) =>
        projectile.Animation is FallingSparkAnimation animation ? animation.Cursor.Program
            : throw new InvalidOperationException("The spark runs no typed program.");

    /// <summary>Runs the falling spark's program oracle and behavior suite.</summary>
    [VerificationCommand("--falling-spark-programs")]
    internal static int FallingSparkProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(FallingSparkAnimationsVerification.VerifyFallingSparkAnimations), () => FallingSparkAnimationsVerification.VerifyFallingSparkAnimations(rom));
        Suite(nameof(VerifyFallingSparkBehavior), () => VerifyFallingSparkBehavior(rom));
        return 0;
    }
}
