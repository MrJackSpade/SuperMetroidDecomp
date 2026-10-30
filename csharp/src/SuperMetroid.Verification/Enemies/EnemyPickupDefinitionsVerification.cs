using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.Deaths.EnemyDeathProjectilePrograms;
using SuperMetroid.Verification.Enemies.Deaths;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks the six native enemy pickup selectors and five live pickup initializers.</summary>
internal static class EnemyPickupDefinitionsVerification
{
    /// <summary>Compares compiled pickup programs with bank-$86, then confirms production spawn timing, pre-instruction, lifetime, and contact behavior.</summary>
    internal static void VerifyEnemyPickupDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 6; index++)
        {
            var kind = (EnemyPickupKind)index;
            EnemyPickupAnimationDefinition animation = EnemyPickups.Animation(kind);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86ef04 + index * 2)),
                animation.Program is { } program ? EnemyDeathProjectileNativeListAddresses.Entry(program) : (ushort)0,
                $"enemy pickup program {kind}");
        }

        for (int index = 1; index <= 5; index++)
        {
            var kind = (EnemyPickupKind)index;
            var projectile = new RoomEnemyProjectileSlot(index)
            {
                InstructionTimer = WrappingCountdown.Start(0xffff),
                PreInstruction = EnemyProjectilePreInstruction.None,
                PersistsOnSamusContact = true,
            };
            EnemyDeathsAndDropsConstants.BeginEnemyPickup(projectile, kind);
            EnemyPickupAnimationDefinition expected = EnemyPickups.Animation(kind);
            AssertEqual(kind, projectile.StateOf<EnemyPickupState>().Kind,
                $"production pickup kind {kind}");
            AssertEqual(expected.Program, ProgramOf(projectile),
                $"production pickup program {kind}");
            AssertEqual(1, projectile.InstructionTimer.RawValue,
                $"production pickup instruction timer {kind}");
            AssertEqual(400, projectile.StateOf<EnemyPickupState>().Lifetime,
                $"production pickup lifetime {kind}");
            AssertEqual(EnemyProjectilePreInstruction.Pickup,
                projectile.PreInstruction,
                $"production pickup pre-instruction {kind}");
            AssertTrue(!projectile.PersistsOnSamusContact,
                $"production pickup contact persistence {kind}");
        }

        AssertThrows<InvalidDataException>(
            () => EnemyPickups.Animation(EnemyPickupKind.NoDrop),
            "no-drop pickup has no animation entry");
        Console.WriteLine(
            "Enemy pickup definitions: six native selectors and all five live pickup initialization paths pass without the pointer table.");
    }

    /// <summary>Runs the pickup selector and initialization checks against the retail cartridge.</summary>
    [VerificationCommand("--enemy-pickup-definitions")]
    internal static int EnemyPickupDefinitionsCommand()
    {
        Suite(nameof(VerifyEnemyPickupDefinitions),
            () => VerifyEnemyPickupDefinitions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
