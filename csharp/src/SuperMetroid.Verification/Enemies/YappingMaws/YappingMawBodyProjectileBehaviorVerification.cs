using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>#1292: the Yapping Maw's body links run the typed program of its facing.</summary>
internal static class YappingMawBodyProjectileBehaviorVerification
{
    /// <summary>Each facing's initializer starts all four body links on that facing's program, matching the native list.</summary>
    internal static void VerifyYappingMawBodyProjectileBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, YappingMawBodyProjectileNativeListAddresses.Extent, "Yapping Maw body");
        foreach ((ushort facing, YappingMawBodyProjectileSegment program) in new VerifyYappingMawBodyProjectileBehaviorCase[]
                 { new VerifyYappingMawBodyProjectileBehaviorCase(0, YappingMawBodyProjectileSegment.FacingDown), new VerifyYappingMawBodyProjectileBehaviorCase(1, YappingMawBodyProjectileSegment.FacingUp) })
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace owner = enemies.Places.InUpdateOrder[0];
            owner.Bind(EnemyDefinitionId.YappingMaw, owner.Definition);
            owner.Bind(owner.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            owner.SetSpawnParameter1(EnemyDefinitionId.YappingMaw, 128);
            owner.SetSpawnParameter2(EnemyDefinitionId.YappingMaw, facing);
            // A room places a Maw inside it, where its root, a few pixels away, lies inside the room too (#2449).
            owner.Position = owner.Position.WithWhole(new(128, 128));
            enemies.YappingMawBehavior.InitializeYappingMaw(owner.HeldOccupant(), owner);
            EnemyProjectile[] links = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.Kind == RoomEnemyProjectileKind.YappingMawBody)];
            AssertEqual(4, links.Length, $"facing {facing} spawns four body links");
            foreach (EnemyProjectile link in links)
            {
                AssertTrue(link.Animation is YappingMawBodyProjectileAnimation animation && animation.PlayingSegment == program, $"facing {facing} starts {program}");
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
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(YappingMawBodyProjectileAnimationsVerification.VerifyYappingMawBodyProjectileAnimations), () => YappingMawBodyProjectileAnimationsVerification.VerifyYappingMawBodyProjectileAnimations(rom));
        Suite(nameof(VerifyYappingMawBodyProjectileBehavior), () => VerifyYappingMawBodyProjectileBehavior(rom));
        return 0;
    }
}
