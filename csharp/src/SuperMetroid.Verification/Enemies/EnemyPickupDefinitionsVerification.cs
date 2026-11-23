using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.Deaths.EnemyDeathProjectilePrograms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks the six native enemy pickup selectors and five live pickup initializers.</summary>
internal static class EnemyPickupDefinitionsVerification
{
    /// <summary>Compares compiled pickup programs with bank-$86, then confirms production spawn timing, pre-instruction, lifetime, and contact behavior.</summary>
    internal static void VerifyEnemyPickupDefinitions(CartridgeImportAddressSpace rom)
    {
        // Selector zero is the program table's empty first entry, which no pickup kind names.
        AssertEqual((ushort)0, rom.ReadCartridgeWord(0x86ef04), "enemy pickup program table's empty first entry");
        AssertThrows<InvalidDataException>(() => EnemyPickupNativeWords.Selectors.Decode(0, "enemy pickup selector"),
            "no pickup kind names selector zero");
        for (int index = 1; index < 6; index++)
        {
            EnemyPickupKind kind = EnemyPickupNativeWords.Selectors.Decode(index, "enemy pickup selector");
            EnemyPickupAnimationDefinition animation = EnemyPickups.Animation(kind);
            AssertEqual(rom.ReadCartridgeWord((0x86ef04 + index * 2)),
                EnemyDeathProjectileNativeListAddresses.Entry(animation.Animation),
                $"enemy pickup program {kind}");
        }

        // A pickup runs the room's deaths and drops, which carry out its animation's events.
        RoomEnemySystem enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance);
        for (int index = 1; index <= 5; index++)
        {
            EnemyPickupKind kind = EnemyPickupNativeWords.Selectors.Decode(index, "enemy pickup selector");
            var projectile = new EnemyProjectile(index, default, RoomEnemyProjectileKind.EnemyDeathPickup, global::SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileBehavior.Still, SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileDeleteAnimation.Instance)
            {
                FrameHold = WrappingCountdown.Start(0xffff),

                PersistsOnSamusContact = true,
            };
            EnemyDeathsAndDropsConstants.BeginEnemyPickup(enemies.DeathsAndDrops, projectile, kind);
            EnemyPickupAnimationDefinition expected = EnemyPickups.Animation(kind);
            AssertEqual(kind, enemies.DeathsAndDrops.Pickups.Of(projectile).Kind,
                $"production pickup kind {kind}");
            AssertEqual(expected.Animation, ProgramOf(projectile),
                $"production pickup program {kind}");
            AssertEqual(1, projectile.FrameHold.RawValue,
                $"production pickup instruction timer {kind}");
            AssertEqual(400, enemies.DeathsAndDrops.Pickups.Of(projectile).Lifetime.UpdatesLeft,
                $"production pickup lifetime {kind}");
            AssertTrue(projectile.MovesBy(SingleMotion.Moving),
                $"production pickup pre-instruction {kind}");
            AssertTrue(!projectile.PersistsOnSamusContact,
                $"production pickup contact persistence {kind}");
        }

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
