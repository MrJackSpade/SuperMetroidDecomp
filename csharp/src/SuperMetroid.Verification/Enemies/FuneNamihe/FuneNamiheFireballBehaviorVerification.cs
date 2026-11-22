using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>#1291: Fune's and Namihe's fireballs run their typed programs and resolve their selectors through its frames.</summary>
internal static class FuneNamiheFireballBehaviorVerification
{
    /// <summary>
    /// Both kinds in both directions start the facing's typed program, which loops update for update beside the native
    /// list; every pose's calculated selector is its native spritemap.
    /// </summary>
    internal static void VerifyFuneNamiheFireballBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = rom;
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, FuneNamiheFireballNativeListAddresses.Extent, "Fune/Namihe fireball");
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace source = enemies.Places.InUpdateOrder[0];
        SuperMetroid.Core.Samus.SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        foreach (RoomEnemyProjectileKind kind in new[] { RoomEnemyProjectileKind.FuneFireball, RoomEnemyProjectileKind.NamiheFireball })
        {
            // Each species spits its own fireball.
            EnemyDefinitionId species = kind == RoomEnemyProjectileKind.FuneFireball ? EnemyDefinitionId.Fune : EnemyDefinitionId.Namihe;
            foreach (bool movingRight in new[] { false, true })
            {
                foreach (EnemyProjectile candidate in enemies.Projectiles.EnemyProjectiles)
                    candidate.Delete();
                // The spitter faces the way its fireball flies, spits on its first update (a Fune whose cooldown is one
                // update, a Namihe whose proximity reaches Samus) and spits its fireball where its spit animation does.
                HorizontalDirection facing = movingRight ? HorizontalDirection.Right : HorizontalDirection.Left;
                EnemySpawnSettings settings = species == EnemyDefinitionId.Namihe
                    ? new NamiheSpawnSettings(facing, SignedEightEight.Zero, YProximity: 1024)
                    : new FuneSpawnSettings(facing, CooldownTime: 1, SignedEightEight.Zero);
                source.PlaceAs<IEnemyEntity>(species, settings, samus, new(288, 128));
                for (int update = 0; !enemies.Projectiles.EnemyProjectiles.Any(candidate => candidate.IsActive && candidate.Kind == kind); update++)
                {
                    AssertTrue(update < 256, $"the {species} facing {facing} spits its fireball");
                    source.RunUpdate(samus);
                }
                EnemyProjectile fireball = enemies.Projectiles.EnemyProjectiles.Single(candidate => candidate.IsActive && candidate.Kind == kind);
                FuneNamiheFireballSegment program = movingRight ? FuneNamiheFireballSegment.Right : FuneNamiheFireballSegment.Left;
                AssertTrue(fireball.Animation is FuneNamiheFireballAnimation animation && animation.PlayingSegment == program,
                    $"the {kind} fireball moving {(movingRight ? "right" : "left")} starts {program}");
                (bool deleted, _) = NativeEnemyProjectileParity.Compare(cartridge, enemies, fireball,
                    NativeEnemyProjectileProgramOracle.EntryOf(FuneNamiheFireballNativeListAddresses.Entries, program), 30, $"{kind} {program}");
                AssertTrue(!deleted, $"{program} loops");
            }
        }
        foreach (EnemyProjectileFrameId frame in FuneNamiheFireballFrameToolingDefinitions.Frames)
        {
            AssertTrue(CompiledEnemyVisualSelectors.TryGet((byte)EnemyInstructionBank.Bank86, EnemyProjectileProgramFrameSources.Operand(frame), out ushort selected),
                $"frame {frame} has a calculated selector");
            AssertEqual(FuneNamiheFireballFrameToolingDefinitions.Spritemap(frame), selected, $"frame {frame}'s calculated selector is its native spritemap");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the fireball's native list bytes");
        Console.WriteLine("  Fune/Namihe fireball behavior: both kinds and directions loop as their native lists do and every selector matches.");
    }

    /// <summary>Runs the Fune/Namihe fireball's program oracle and behavior suite.</summary>
    [VerificationCommand("--fune-namihe-fireball-programs")]
    internal static int FuneNamiheFireballProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(FuneNamiheFireballAnimationsVerification.VerifyFuneNamiheFireballAnimations), () => FuneNamiheFireballAnimationsVerification.VerifyFuneNamiheFireballAnimations(rom));
        Suite(nameof(VerifyFuneNamiheFireballBehavior), () => VerifyFuneNamiheFireballBehavior(rom));
        return 0;
    }
}
