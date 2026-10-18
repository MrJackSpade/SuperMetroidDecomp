using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>#1291: Alcoon's fireballs run their typed program and resolve their calculated selectors through its frames.</summary>
internal static class AlcoonFireballBehaviorVerification
{
    /// <summary>
    /// All three fire commands in both facings start the typed program, which loops update for update beside the native
    /// list; the shared shot list still deletes a fireball; every pose's calculated selector is its native spritemap.
    /// </summary>
    internal static void VerifyAlcoonFireballBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, AlcoonFireballNativeListAddresses.Extent, "Alcoon-fireball");
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot source = enemies.SlotTable.Slots[0];
        source.Bind(EnemyDefinitionId.Alcoon, source.Definition);
        source.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112));
        source.VramTilesIndex = 512;
        source.PaletteIndex = 3072;
        var state = new AlcoonEnemyState();
        source.SetSpeciesState(state);
        foreach (short direction in new short[] { -2, 2 })
        {
            state.Velocity = state.Velocity.WithWholeX(direction);
            foreach (ushort velocityOffset in new ushort[] { 0, 2, 4 })
                enemies.AlcoonBehavior.SpawnAlcoonFireball(source, velocityOffset);
        }
        RoomEnemyProjectileSlot[] fireballs = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball)];
        AssertEqual(6, fireballs.Length, "all three fire commands spawn in both facings");
        foreach (RoomEnemyProjectileSlot fireball in fireballs)
        {
            AssertTrue(fireball.Animation is AlcoonFireballAnimation { Cursor.Program: AlcoonFireballProgram.Initial }, "a fireball starts its typed program");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, fireball,
                NativeEnemyProjectileProgramOracle.EntryOf(AlcoonFireballNativeListAddresses.Entries, AlcoonFireballProgram.Initial), 30, "Alcoon fireball");
            AssertTrue(!deleted, "a fireball loops until it is shot or leaves");
        }
        fireballs[0].StartAnimation(EnemyProjectileDeleteAnimation.Instance);
        enemies.Projectiles.ProcessEnemyProjectileInstructions(fireballs[0], null, Position16<RoomPixels>.Zero);
        AssertTrue(!fireballs[0].IsActive, "the shared shot list still deletes a fireball");
        for (int pose = 0; pose < AlcoonFireballFrameDefinitions.PoseCount; pose++)
        {
            AlcoonFireballFrame frame = new(pose);
            ushort key = EnemyProjectileFrames.ProgramFrameKey(AlcoonFireballFrameDefinitions.ShownProjectileFrames[frame]);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet((byte)EnemyInstructionBank.Bank86, key, out ushort selected), $"pose {pose} has a calculated selector");
            AssertEqual(AlcoonFireballFrameDefinitions.Spritemap(frame), selected, $"pose {pose}'s calculated selector is its native spritemap");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the fireball's native list bytes");
        Console.WriteLine("  Alcoon fireball behavior: six fireballs loop as their native list does and every calculated selector matches.");
    }

    /// <summary>Runs the Alcoon fireball's program oracle and behavior suite.</summary>
    [VerificationCommand("--alcoon-fireball-programs")]
    internal static int AlcoonFireballProgramsCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(AlcoonFireballAnimationsVerification.VerifyAlcoonFireballAnimations), () => AlcoonFireballAnimationsVerification.VerifyAlcoonFireballAnimations(rom));
        Suite(nameof(VerifyAlcoonFireballBehavior), () => VerifyAlcoonFireballBehavior(rom));
        return 0;
    }
}
