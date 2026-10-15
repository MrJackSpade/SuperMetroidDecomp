using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.Deaths.EnemyDeathProjectilePrograms;
using SuperMetroid.Verification.Enemies.Deaths;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks the six native enemy pickup selectors and five live pickup initializers.</summary>
internal static class EnemyPickupDefinitionsVerification
{
    /// <summary>Compares compiled animation offsets and pointers with bank-$86, then confirms production spawn timing, pre-instruction, lifetime, and contact behavior.</summary>
    internal static void VerifyEnemyPickupDefinitions(SuperMetroidAddressSpace rom)
    {
        for (int index = 0; index < 6; index++)
        {
            var kind = (EnemyPickupKind)index;
            EnemyPickupAnimationDefinition animation = EnemyPickupDefinitions.Animation(kind);
            AssertEqual(index * 2, animation.TableOffset,
                $"enemy pickup table offset {kind}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86ef04 + index * 2)),
                animation.Program is { } program ? EnemyDeathProjectileNativeListAddresses.Entry(program) : (ushort)0,
                $"enemy pickup program {kind}");
        }

        for (int index = 1; index <= 5; index++)
        {
            var kind = (EnemyPickupKind)index;
            var projectile = new RoomEnemyProjectileSlot(index)
            {
                Variable0 = 0xffff,
                Variable1 = 0xffff,
                InstructionPointer = 0xffff,
                InstructionTimer = NativeCountdown.Start(0xffff),
                PreInstruction = 0xffff,
                PersistsOnSamusContact = true,
            };
            EnemyDeathsAndDrops.BeginEnemyPickup(projectile, kind);
            EnemyPickupAnimationDefinition expected = EnemyPickupDefinitions.Animation(kind);
            AssertEqual(expected.TableOffset, projectile.Variable0,
                $"production pickup table offset {kind}");
            AssertEqual(expected.Program, ProgramOf(projectile),
                $"production pickup program {kind}");
            AssertEqual(1, projectile.InstructionTimer.RawValue,
                $"production pickup instruction timer {kind}");
            AssertEqual(400, projectile.Variable1,
                $"production pickup lifetime {kind}");
            AssertEqual((ushort)EnemyProjectilePreInstruction.Pickup,
                projectile.PreInstruction,
                $"production pickup pre-instruction {kind}");
            AssertTrue(!projectile.PersistsOnSamusContact,
                $"production pickup contact persistence {kind}");
        }

        AssertThrows<InvalidDataException>(
            () => EnemyPickupDefinitions.Animation(EnemyPickupKind.NoDrop),
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
