using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Skree;

/// <summary>#1291: the Skree's and Metaree's death particles run typed programs that draw the shared debris compositions.</summary>
internal static class SkreeMetareeParticleBehaviorVerification
{
    /// <summary>
    /// Each burst spawns four particles of its family on that family's typed program, keeping the definition's contact
    /// damage; every particle matches its native list update for update, drawing the composition the native operand names.
    /// </summary>
    internal static void VerifySkreeMetareeParticleBehavior(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var guard = NativeEnemyProjectileListGuard.Forbidding(rom, SkreeMetareeParticleNativeListAddresses.Extent, "Skree/Metaree particle");
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var source = new RoomEnemySlot(0, EnemySlotFixtures.Registry)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)),
            VramTilesIndex = 512,
            PaletteIndex = 3072,
        };
        foreach ((Action<RoomEnemySlot> spawn, SkreeMetareeParticleProgram program) in new VerifySkreeMetareeParticleBehaviorCase[]
                 {
                     new VerifySkreeMetareeParticleBehaviorCase(enemies.Projectiles.SpawnSkreeParticleBurst, SkreeMetareeParticleProgram.Skree),
                     new VerifySkreeMetareeParticleBehaviorCase(enemies.Projectiles.SpawnMetareeParticleBurst, SkreeMetareeParticleProgram.Metaree),
                 })
        {
            foreach (RoomEnemyProjectileSlot stale in enemies.Projectiles.EnemyProjectiles)
                stale.Clear();
            spawn(source);
            RoomEnemyProjectileSlot[] particles = [.. enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.IsActive)];
            AssertEqual(4, particles.Length, $"the {program} burst spawns four particles");
            foreach (RoomEnemyProjectileSlot particle in particles)
            {
                AssertTrue(particle.Animation is SkreeMetareeParticleAnimation animation && animation.Cursor.Program == program,
                    $"the {particle.Kind} particle starts the {program} program");
                // SpawnEprojInner copies the definition's properties: four contact damage.
                AssertTrue(particle.CanDamageSamus && particle.Damage == 4 && particle.InvincibilityFrames == 96,
                    $"the {particle.Kind} particle keeps its definition's contact damage");
                var native = new NativeEnemyProjectileListDefinitions.Reader(SkreeMetareeParticleNativeListAddresses.Entry(program), particle.InstructionTimer.RawValue);
                for (int update = 0; update < 40; update++)
                {
                    enemies.Projectiles.ProcessEnemyProjectileInstructions(particle, new SamusState(), Position16<RoomPixels>.Zero);
                    native.Step(cartridge);
                    string at = $"{particle.Kind} update {update}";
                    AssertTrue(!native.Deleted && particle.IsActive, $"{at}: the particle lives");
                    AssertEqual(native.Timer, particle.InstructionTimer.RawValue, $"{at}: timer");
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
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(SkreeMetareeParticleAnimationsVerification.VerifySkreeMetareeParticleAnimations),
            () => SkreeMetareeParticleAnimationsVerification.VerifySkreeMetareeParticleAnimations(rom));
        Suite(nameof(VerifySkreeMetareeParticleBehavior), () => VerifySkreeMetareeParticleBehavior(rom));
        return 0;
    }
}
