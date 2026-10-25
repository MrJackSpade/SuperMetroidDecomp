using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.Deaths.EnemyDeathProjectilePrograms;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Deaths;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Deaths.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Deaths;

/// <summary>#1291: enemy deaths, the drops they become and the respawn tail run typed programs.</summary>
internal static class EnemyDeathProjectileBehaviorVerification
{
    /// <summary>
    /// Every death variant starts its explosion program; a small explosion runs to its conversion and continues on that
    /// same pass into the rolled drop's program, showing the drop's first frame; the drop's expiry starts the respawn
    /// tail, which deletes the drop after its blank frame.
    /// </summary>
    internal static void VerifyEnemyDeathProjectileBehavior()
    {
        for (ushort animation = 0; animation < 5; animation++)
        {
            EnemyDropFixture variant = CreateEnemyDropFixture(CreateDropTestSamus(), randomValues: []);
            RoomEnemySlot enemy = variant.System.SlotTable.Slots[0];
            enemy.Bind(EnemyDefinitionId.Sciser, enemy.Definition);
            variant.System.DeathsAndDrops.StartGenericEnemyDeath(enemy, animation);
            RoomEnemyProjectileSlot explosion = variant.System.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
            AssertEqual(EnemyDeathExplosionDefinitions.Program(animation), ProgramOf(explosion), $"death variant {animation} starts its program");
        }

        SamusState samus = CreateDropTestSamus();
        samus.Health = 50;
        EnemyDropFixture fixture = CreateEnemyDropFixture(samus, randomValues: [1]);
        const ushort enemyHeader = 36864;
        SeedEnemyDropChancePointer(fixture.Bus, enemyHeader);
        RoomEnemySlot killed = fixture.System.SlotTable.Slots[0];
        killed.Bind((EnemyDefinitionId)enemyHeader, killed.Definition);
        killed.Position = killed.Position.WithWhole(new(320, 120));
        fixture.System.DeathsAndDrops.StartGenericEnemyDeath(killed, deathAnimation: 0);
        RoomEnemyProjectileSlot dying = fixture.System.Projectiles.EnemyProjectiles.Single(projectile => projectile.IsActive);
        int updates = 0;
        while (ProgramOf(dying) == EnemyDeathProjectileProgram.SmallExplosion)
        {
            fixture.System.Projectiles.ProcessEnemyProjectileInstructions(dying, samus, Position16<RoomPixels>.Zero);
            AssertTrue(++updates < 64, "the small explosion converts within its six frames");
        }
        AssertEqual(EnemyDeathProjectileProgram.SmallEnergy, ProgramOf(dying), "the conversion continues at the rolled small-energy drop");
        AssertEqual(EnemyProjectileProgramFrameSources.Operand(EnemyDeathProjectileFrameDefinitions.ShownProjectileFrames[new(EnemyProjectilePresentationFrameDefinitions.DeathFrameCount)]),
            dying.PresentationOperandAddress, "the drop's first frame shows on the conversion's own pass");

        dying.Variable1 = 1;
        fixture.System.DeathsAndDrops.RunEnemyPickupPreInstruction(dying, samus);
        AssertEqual(EnemyDeathProjectileProgram.RespawnTail, ProgramOf(dying), "an expired drop starts the respawn tail");
        for (int update = 0; update < 70 && dying.IsActive; update++)
            fixture.System.Projectiles.ProcessEnemyProjectileInstructions(dying, samus, Position16<RoomPixels>.Zero);
        AssertTrue(!dying.IsActive, "the respawn tail deletes the drop after its blank frame");
        Console.WriteLine("  Enemy death behavior: every variant's program, the same-pass drop conversion and the respawn tail pass.");
    }

    /// <summary>Runs the death and drop programs' oracle and behavior suite.</summary>
    [VerificationCommand("--enemy-death-projectile-programs")]
    internal static int EnemyDeathProjectileProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(EnemyDeathProjectileAnimationsVerification.VerifyEnemyDeathProjectileAnimations),
            () => EnemyDeathProjectileAnimationsVerification.VerifyEnemyDeathProjectileAnimations(rom));
        Suite(nameof(VerifyEnemyDeathProjectileBehavior), VerifyEnemyDeathProjectileBehavior);
        return 0;
    }
}
