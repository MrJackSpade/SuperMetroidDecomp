using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Input;
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
        IImportCartridgeSource cartridge = rom;
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, AlcoonFireballNativeListAddresses.Extent, "Alcoon-fireball");
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The Alcoon is placed as its room's population places it, standing in terrain that its setup's floor search meets at
        // once; its volley's animation then spits its three fireballs, one at each of its fire cues, facing each way in turn.
        RoomEnemyPlace source = enemies.Places.InUpdateOrder[0];
        AlcoonEnemyState state = enemies.PlaceAs<AlcoonEntity>(EnemyDefinitionId.Alcoon, EnemySpawnSettings.None, RoomEnemyFrameFixtures.StandaloneSamus(),
            new Position16<RoomPixels>(128, 0x300), PlacedEnemyAccess.SolidRoom(new(64, 64))).State;
        source.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 112));
        source.Shading = SuperMetroid.Core.Enemies.Drawing.EnemyShades.FrozenLook;
        object?[] arguments = [source, null, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
        foreach ((AlcoonWalk direction, AlcoonSegment volley) in new[] { (AlcoonWalk.Left, AlcoonSegment.FireLeft), (AlcoonWalk.Right, AlcoonSegment.FireRight) })
        {
            state.WalkingDirection = direction;
            state.Playback = new(AlcoonAnimations.StartingWith(volley), AnimationPlace.StartOfAnimation);
            source.AnimationClock.StepOnNextUpdate();
            int spat = enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball) + 3;
            for (int update = 0; update < 200 && enemies.Projectiles.EnemyProjectiles.Count(projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball) < spat; update++)
                EnemyInstructionRunner.Process(arguments);
        }
        EnemyProjectile[] fireballs = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball)];
        AssertEqual(6, fireballs.Length, "all three fire commands spawn in both facings");
        foreach (EnemyProjectile fireball in fireballs)
        {
            AssertTrue(fireball.Animation is AlcoonFireballAnimation { PlayingSegment: AlcoonFireballSegment.Spinning }, "a fireball starts its typed program");
            (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, fireball,
                NativeEnemyProjectileProgramOracle.EntryOf(AlcoonFireballNativeListAddresses.Entries, AlcoonFireballSegment.Spinning), 30, "Alcoon fireball");
            AssertTrue(!deleted, "a fireball loops until it is shot or leaves");
        }
        fireballs[0].StartAnimation(EnemyProjectileDeleteAnimation.Instance);
        enemies.Projectiles.AdvanceEnemyProjectileAnimations(fireballs[0], RoomEnemyFrameFixtures.StandaloneSamus(), Position16<RoomPixels>.Zero, shortCounter: SuperMetroid.Core.Compatibility.ShortFrameCount.Zero);
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
