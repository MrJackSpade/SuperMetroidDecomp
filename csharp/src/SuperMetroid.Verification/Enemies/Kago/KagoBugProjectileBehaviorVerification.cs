using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Kago;

/// <summary>#1292: the Kago's bugs run typed landing, falling and jumping programs.</summary>
internal static class KagoBugProjectileBehaviorVerification
{
    /// <summary>Each of the bug's landing, falling and jumping programs matches its native list update for update, its cues included.</summary>
    internal static void VerifyKagoBugProjectileBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, KagoBugProjectileNativeListAddresses.Extent, "Kago bug");
        foreach (KagoBugProjectileProgram program in new[] { KagoBugProjectileProgram.Landed, KagoBugProjectileProgram.Falling, KagoBugProjectileProgram.JumpStart })
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 1);
            RoomEnemyProjectileSlot bug = enemies.Projectiles.EnemyProjectiles[0];
            bug.Kind = RoomEnemyProjectileKind.KagoBug;
            bug.StartAnimation(new KagoBugProjectileAnimation(program));
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, bug, KagoBugProjectileNativeListAddresses.Entry(program), 40, program.ToString());
            AssertTrue(!deleted, $"{program} keeps the bug");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the bugs' native list bytes");
        Console.WriteLine("  Kago bug behavior: the landing, falling and jumping programs match their native lists.");
    }

    /// <summary>Runs the bugs' program oracle and behavior suite.</summary>
    [VerificationCommand("--kago-bug-programs")]
    internal static int KagoBugProjectileProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(KagoBugProjectileAnimationsVerification.VerifyKagoBugProjectileAnimations), () => KagoBugProjectileAnimationsVerification.VerifyKagoBugProjectileAnimations(rom));
        Suite(nameof(VerifyKagoBugProjectileBehavior), () => VerifyKagoBugProjectileBehavior(rom));
        return 0;
    }
}
