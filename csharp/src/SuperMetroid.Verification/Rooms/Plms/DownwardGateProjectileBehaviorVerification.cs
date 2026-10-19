using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>#1292: the downward shot gate runs typed programs that its movement steps past each sleep.</summary>
internal static class DownwardGateProjectileBehaviorVerification
{
    /// <summary>
    /// The moving gate's definition starts its typed program; each block its movement routine completes resumes the
    /// program after the sleep, and the whole chain, through the closed gate's descent and deletion, matches the native
    /// list the two-byte advance walks.
    /// </summary>
    internal static void VerifyDownwardGateProjectileBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, DownwardGateProjectileNativeListAddresses.Extent, "downward gate");
        var enemies = new RoomEnemySystem(new EnemySession());
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyProjectileSlot gate = enemies.Projectiles.EnemyProjectiles[0];
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(gate, RoomEnemyProjectileKind.DownwardGateMoving, 0);
        AssertTrue(gate.Animation is DownwardGateProjectileAnimation { Cursor.Program: DownwardGateProjectileProgram.Moving }, "the moving gate starts its program");

        DownwardGateProjectileProgram current = DownwardGateProjectileProgram.Moving;
        int resumes = 0;
        while (true)
        {
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, gate,
                DownwardGateProjectileNativeListAddresses.Entry(current), 3, current.ToString());
            if (deleted)
                break;
            current = gate.Animation is DownwardGateProjectileAnimation sleeping
                ? sleeping.Cursor.Program
                : throw new InvalidOperationException("The gate lost its typed program.");
            gate.GeneralTimer = DownwardGateEnemyProjectileRomData.OneBlockDistance - 1;
            gate.Velocity = gate.Velocity.WithY(new SnesSignedEightEight(256));
            DownwardGateBehavior.RunDownwardGateProjectileMovement(gate);
            DownwardGateProjectileProgram resumed = DownwardGateProjectileAnimations.Resumption[current];
            AssertTrue(gate.Animation is DownwardGateProjectileAnimation next && next.Cursor.Program == resumed, $"a block's travel resumes {current} with {resumed}");
            AssertEqual((ushort)1, gate.InstructionTimer.RawValue, $"{resumed} wakes on the same pass");
            current = resumed;
            resumes++;
        }
        AssertEqual(DownwardGateProjectileAnimations.Resumption.Count, resumes, "every sleep is resumed once before the gate deletes itself");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the gate's native list bytes");
        Console.WriteLine($"  Downward gate behavior: {resumes} resumed sleeps carry the moving and closed programs to deletion as the native list does.");
    }

    /// <summary>Runs the gate's program oracle and behavior suite.</summary>
    [VerificationCommand("--downward-gate-projectile-programs")]
    internal static int DownwardGateProjectileProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(DownwardGateProjectileAnimationsVerification.VerifyDownwardGateProjectileAnimations), () => DownwardGateProjectileAnimationsVerification.VerifyDownwardGateProjectileAnimations(rom));
        Suite(nameof(VerifyDownwardGateProjectileBehavior), () => VerifyDownwardGateProjectileBehavior(rom));
        return 0;
    }
}
