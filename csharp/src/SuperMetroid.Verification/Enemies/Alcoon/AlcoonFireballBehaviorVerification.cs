using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>#1291: Alcoon's fireballs run their typed program and resolve their calculated selectors through its frames.</summary>
internal static class AlcoonFireballBehaviorVerification
{
    /// <summary>
    /// All three fire commands in both facings start the typed program, which loops update for update beside the native
    /// list; the shared shot list still deletes a fireball; every pose's calculated selector is its native spritemap.
    /// </summary>
    internal static void VerifyAlcoonFireballBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, AlcoonFireballNativeListAddresses.Extent, "Alcoon-fireball");
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot source = enemies.SlotTable.Slots[0];
        source.Bind(EnemyDefinitionId.Alcoon, source.Definition);
        source.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112));
        source.Shading = SuperMetroid.Core.Enemies.Drawing.EnemyShades.FrozenLook;
        var state = new AlcoonEnemyState();
        source.SetSpeciesState(state);
        foreach (short direction in new short[] { -2, 2 })
        {
            state.Velocity = state.Velocity.WithWholeX(direction);
            foreach (AlcoonFireballLaunch launch in new[] { AlcoonFireballLaunchDefinitions.Upward, AlcoonFireballLaunchDefinitions.Level, AlcoonFireballLaunchDefinitions.Downward })
                enemies.AlcoonBehavior.SpawnAlcoonFireball(source, launch);
        }
        EnemyProjectile[] fireballs = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball)];
        AssertEqual(6, fireballs.Length, "all three fire commands spawn in both facings");
        foreach (EnemyProjectile fireball in fireballs)
        {
            AssertTrue(fireball.Animation is AlcoonFireballAnimation { PlayingSegment: AlcoonFireballSegment.Initial }, "a fireball starts its typed program");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, fireball,
                NativeEnemyProjectileProgramOracle.EntryOf(AlcoonFireballNativeListAddresses.Entries, AlcoonFireballSegment.Initial), 30, "Alcoon fireball");
            AssertTrue(!deleted, "a fireball loops until it is shot or leaves");
        }
        fireballs[0].StartAnimation(EnemyProjectileDeleteAnimation.Instance);
        enemies.Projectiles.AdvanceEnemyProjectileAnimations(fireballs[0], null, Position16<RoomPixels>.Zero);
        AssertTrue(!fireballs[0].IsActive, "the shared shot list still deletes a fireball");
        foreach (EnemyProjectileFrameId frame in AlcoonFireballFrameToolingDefinitions.Frames)
        {
            ushort key = EnemyProjectileProgramFrameSources.Operand(frame);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet((byte)EnemyInstructionBank.Bank86, key, out ushort selected), $"frame {frame} has a calculated selector");
            AssertEqual(AlcoonFireballFrameToolingDefinitions.Spritemap(frame), selected, $"frame {frame}'s calculated selector is its native spritemap");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the fireball's native list bytes");
        Console.WriteLine("  Alcoon fireball behavior: six fireballs loop as their native list does and every calculated selector matches.");
    }

    /// <summary>Runs the Alcoon fireball's program oracle and behavior suite.</summary>
    [VerificationCommand("--alcoon-fireball-programs")]
    internal static int AlcoonFireballProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(AlcoonFireballAnimationsVerification.VerifyAlcoonFireballAnimations), () => AlcoonFireballAnimationsVerification.VerifyAlcoonFireballAnimations(rom));
        Suite(nameof(VerifyAlcoonFireballBehavior), () => VerifyAlcoonFireballBehavior(rom));
        return 0;
    }
}
