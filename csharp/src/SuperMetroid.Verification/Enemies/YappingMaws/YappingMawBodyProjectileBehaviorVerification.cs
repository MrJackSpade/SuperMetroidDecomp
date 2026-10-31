using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>#1292: the Yapping Maw's body links run the typed program of its facing.</summary>
internal static class YappingMawBodyProjectileBehaviorVerification
{
    /// <summary>Each facing's initializer starts all four body links on that facing's program, matching the native list.</summary>
    internal static void VerifyYappingMawBodyProjectileBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, YappingMawBodyProjectileNativeListAddresses.Extent, "Yapping Maw body");
        foreach ((ushort facing, YappingMawBodyProjectileProgram program) in new VerifyYappingMawBodyProjectileBehaviorCase[]
                 { new VerifyYappingMawBodyProjectileBehaviorCase(0, YappingMawBodyProjectileProgram.FacingDown), new VerifyYappingMawBodyProjectileBehaviorCase(1, YappingMawBodyProjectileProgram.FacingUp) })
        {
            var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot owner = enemies.SlotTable.Slots[0];
            owner.Bind(EnemyDefinitionId.YappingMaw, owner.Definition);
            owner.Bind(owner.DefinitionId, default(RoomEnemyDefinition));
            owner.SetSpawnParameter1(EnemyDefinitionId.YappingMaw, 128);
            owner.SetSpawnParameter2(EnemyDefinitionId.YappingMaw, facing);
            enemies.YappingMawBehavior.InitializeYappingMaw(owner);
            RoomEnemyProjectileSlot[] links = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.Kind == RoomEnemyProjectileKind.YappingMawBody)];
            AssertEqual(4, links.Length, $"facing {facing} spawns four body links");
            foreach (RoomEnemyProjectileSlot link in links)
            {
                AssertTrue(link.Animation is YappingMawBodyProjectileAnimation animation && animation.Cursor.Program == program, $"facing {facing} starts {program}");
                (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, link, YappingMawBodyProjectileNativeListAddresses.Entry(program), 4, program.ToString());
                AssertTrue(!deleted, $"{program} holds its pose");
            }
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the body links' native list bytes");
        Console.WriteLine("  Yapping Maw body behavior: both facings start their programs on all four links and match the native lists.");
    }

    /// <summary>Runs the body links' program oracle and behavior suite.</summary>
    [VerificationCommand("--yapping-maw-body-programs")]
    internal static int YappingMawBodyProjectileProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(YappingMawBodyProjectileAnimationsVerification.VerifyYappingMawBodyProjectileAnimations), () => YappingMawBodyProjectileAnimationsVerification.VerifyYappingMawBodyProjectileAnimations(rom));
        Suite(nameof(VerifyYappingMawBodyProjectileBehavior), () => VerifyYappingMawBodyProjectileBehavior(rom));
        return 0;
    }
}
