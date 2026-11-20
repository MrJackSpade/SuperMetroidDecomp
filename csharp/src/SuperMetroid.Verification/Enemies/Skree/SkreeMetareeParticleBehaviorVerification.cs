using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.MovieReplay.Verification;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>#1291: the Skree's and Metaree's death particles run typed programs that draw the shared debris compositions.</summary>
internal static class SkreeMetareeParticleBehaviorVerification
{
    /// <summary>
    /// Each burst spawns four particles of its family on that family's typed program, keeping the definition's contact
    /// damage; every particle matches its native list update for update, drawing the composition the native operand names.
    /// </summary>
    internal static void VerifySkreeMetareeParticleBehavior(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, SkreeMetareeParticleNativeListAddresses.Extent, "Skree/Metaree particle");
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace source = enemies.Places.InUpdateOrder[0];
        source.Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96));
        source.Shading = SuperMetroid.Core.Enemies.Drawing.EnemyShades.FrozenLook;
        foreach ((Action<RoomEnemyPlace> spawn, SkreeMetareeParticleSegment program) in new VerifySkreeMetareeParticleBehaviorCase[]
                 {
                     new VerifySkreeMetareeParticleBehaviorCase(place => SkreeMetareeParticles.SpawnBurst(enemies.Projectiles, place.BoundFor(EnemyDefinitionId.Skree), place, SkreeMetareeParticleSegment.Skree), SkreeMetareeParticleSegment.Skree),
                     new VerifySkreeMetareeParticleBehaviorCase(place => SkreeMetareeParticles.SpawnBurst(enemies.Projectiles, place.HeldOccupant(), place, SkreeMetareeParticleSegment.Metaree), SkreeMetareeParticleSegment.Metaree),
                 })
        {
            foreach (EnemyProjectile stale in enemies.Projectiles.EnemyProjectiles)
                stale.Delete();
            spawn(source);
            EnemyProjectile[] particles = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.IsActive)];
            AssertEqual(4, particles.Length, $"the {program} burst spawns four particles");
            foreach (EnemyProjectile particle in particles)
            {
                AssertTrue(particle.Animation is SkreeMetareeParticleAnimation animation && animation.PlayingSegment == program,
                    $"the {particle.Kind} particle starts the {program} program");
                // SpawnEprojInner copies the definition's properties: four contact damage.
                AssertTrue(particle.CanDamageSamus && particle.Damage == 4,
                    $"the {particle.Kind} particle keeps its definition's contact damage");
                var native = new NativeEnemyProjectileListReader(SkreeMetareeParticleNativeListAddresses.Entry(program), particle.FrameHold.RawValue);
                ushort? heldOperand = null;
                for (int update = 0; update < 40; update++)
                {
                    enemies.Projectiles.AdvanceEnemyProjectileAnimations(particle, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), Position16<RoomPixels>.Zero, shortCounter: SuperMetroid.Core.Compatibility.ShortFrameCount.Zero);
                    native.Step(cartridge);
                    string at = $"{particle.Kind} update {update}";
                    AssertTrue(!native.Deleted && particle.IsActive, $"{at}: the particle lives");
                    // The debris is a still picture (#2276): the native list replays its one frame, which the typed
                    // particle holds for good, counting no timer; the native list shows that one composition throughout.
                    heldOperand ??= native.Operand;
                    AssertEqual(heldOperand.Value, native.Operand, $"{at}: the native list shows its one frame again");
                    AssertEqual(cartridge.ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank | native.Operand), particle.SpritemapPointer,
                        $"{at}: the drawn composition is the one the native operand names");
                }
            }
        }
        AssertEqual(0, guard.ForbiddenReadAttempts, "production reads none of the particles' native list bytes");
        Console.WriteLine("  Skree/Metaree particle behavior: both bursts run their programs and draw the native compositions update for update.");
    }

    /// <summary>Runs the particles' program oracle and behavior suite.</summary>
    [VerificationCommand("--skree-metaree-particle-programs")]
    internal static int SkreeMetareeParticleProgramsCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(SkreeMetareeParticleAnimationsVerification.VerifySkreeMetareeParticleAnimations),
            () => SkreeMetareeParticleAnimationsVerification.VerifySkreeMetareeParticleAnimations(rom));
        Suite(nameof(VerifySkreeMetareeParticleBehavior), () => VerifySkreeMetareeParticleBehavior(rom));
        return 0;
    }
}
