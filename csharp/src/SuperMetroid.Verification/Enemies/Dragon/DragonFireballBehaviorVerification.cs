using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>#1291: Dragon's fireballs select and switch their typed programs as the native list stores did.</summary>
internal static class DragonFireballBehaviorVerification
{
    /// <summary>
    /// Spawning selects each facing's rising program, which loops update for update beside the native list; the zero
    /// crossing switches to the facing's falling program on the same pass; the shared shot list still deletes the fireball.
    /// </summary>
    internal static void VerifyDragonFireballBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, DragonFireballNativeListAddresses.Extent, "Dragon-fireball");
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);

        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Dragon, body.Definition);
        body.Bind(body.DefinitionId, default(RoomEnemyDefinition) with { Bank = DragonFrameToolingDefinitions.Bank });
        body.Position = body.Position.WithWhole(new(384, 160));
        body.VramTilesIndex = 512;
        body.PaletteIndex = 2560;
        DragonBehavior.InitializeDragon(body);
        DragonEnemyState state = enemies.DragonStates[0] ?? throw new InvalidOperationException("Verification requires the Dragon's state.");

        state.FacingLeft = true;
        enemies.DragonBehavior.SpawnDragonFireball(body, state);
        state.FacingLeft = false;
        enemies.DragonBehavior.SpawnDragonFireball(body, state);
        RoomEnemyProjectileSlot left = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.DragonFireball && projectile.Velocity.X.SignedRawValue < 0);
        RoomEnemyProjectileSlot right = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.DragonFireball && projectile.Velocity.X.SignedRawValue >= 0);
        foreach ((RoomEnemyProjectileSlot fireball, DragonFireballProgram rising, DragonFireballProgram falling) in new[]
                 {
                     new VerifyDragonFireballBehaviorCase(left, DragonFireballProgram.RisingLeft, DragonFireballProgram.FallingLeft),
                     new VerifyDragonFireballBehaviorCase(right, DragonFireballProgram.RisingRight, DragonFireballProgram.FallingRight),
                 })
        {
            AssertEqual(rising, ProgramOf(fireball), $"the fireball starts {rising}");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, fireball, Entry(rising), 25, rising.ToString());
            AssertTrue(!deleted, $"{rising} loops");
            fireball.Velocity = fireball.Velocity.WithY(SnesSignedEightEight.FromSignedRaw(-16));
            DragonBehavior.RunDragonFireballPreInstruction(fireball, 0);
            AssertEqual(falling, ProgramOf(fireball), $"the zero crossing selects {falling}");
            AssertEqual((ushort)1, fireball.InstructionTimer.RawValue, $"the switch to {falling} requests a same-pass frame");
            (deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, fireball, Entry(falling), 25, falling.ToString());
            AssertTrue(!deleted, $"{falling} loops");
        }

        left.StartAnimation(EnemyProjectileDeleteAnimation.Instance);
        enemies.Projectiles.ProcessEnemyProjectileInstructions(left, null, Position16<RoomPixels>.Zero);
        AssertTrue(!left.IsActive, "the shared shot list still deletes the fireball");
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the fireball's native list bytes");
        Console.WriteLine("  Dragon fireball behavior: both rising and falling programs and the zero-crossing switch match the native lists.");
    }

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    private static ushort Entry(DragonFireballProgram program) =>
        NativeEnemyProjectileProgramOracle.EntryOf(DragonFireballNativeListAddresses.Entries, program);

    /// <summary>The typed program <paramref name="projectile"/> runs.</summary>
    private static DragonFireballProgram ProgramOf(RoomEnemyProjectileSlot projectile) =>
        projectile.Animation is DragonFireballAnimation animation ? animation.Cursor.Program
            : throw new InvalidOperationException("The fireball runs no typed program.");

    /// <summary>Runs Dragon's fireball program oracle and behavior suite.</summary>
    [VerificationCommand("--dragon-fireball-programs")]
    internal static int DragonFireballProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(DragonFireballAnimationsVerification.VerifyDragonFireballAnimations), () => DragonFireballAnimationsVerification.VerifyDragonFireballAnimations(rom));
        Suite(nameof(VerifyDragonFireballBehavior), () => VerifyDragonFireballBehavior(rom));
        return 0;
    }
}
