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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var source = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)),
            Shading = SuperMetroid.Core.Enemies.Drawing.EnemyShades.FrozenLook,
        };
        foreach ((Action<RoomEnemySlot> spawn, SkreeMetareeParticleSegment program) in new VerifySkreeMetareeParticleBehaviorCase[]
                 {
                     new VerifySkreeMetareeParticleBehaviorCase(slot => enemies.Projectiles.SpawnMetalParticleBurst(slot, SuperMetroid.Core.Enemies.Skree.SkreeParticleDefinitions.Burst), SkreeMetareeParticleSegment.Skree),
                     new VerifySkreeMetareeParticleBehaviorCase(slot => enemies.Projectiles.SpawnMetalParticleBurst(slot, SuperMetroid.Core.Enemies.Metaree.MetareeParticleDefinitions.Burst), SkreeMetareeParticleSegment.Metaree),
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
                AssertTrue(particle.CanDamageSamus && particle.Damage == 4 && particle.InvincibilityFrames == 96,
                    $"the {particle.Kind} particle keeps its definition's contact damage");
                var native = new NativeEnemyProjectileListReader(SkreeMetareeParticleNativeListAddresses.Entry(program), particle.FrameHold.RawValue);
                for (int update = 0; update < 40; update++)
                {
                    enemies.Projectiles.AdvanceEnemyProjectileAnimations(particle, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), Position16<RoomPixels>.Zero);
                    native.Step(cartridge);
                    string at = $"{particle.Kind} update {update}";
                    AssertTrue(!native.Deleted && particle.IsActive, $"{at}: the particle lives");
                    AssertEqual(native.Timer, particle.FrameHold.RawValue, $"{at}: timer");
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
