using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Drawing;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;

/// <summary>Checks Samus atmospheric animation timers and lava/acid damage rates against bank 90.</summary>
internal static class SamusAtmosphericAnimationDefinitionsVerification
{
    /// <summary>Compares all active frame timers and liquid-damage words with the cartridge, then exercises their consumers.</summary>
    internal static void VerifySamusAtmosphericAnimationDefinitions(
        CartridgeImportAddressSpace rom)
    {

        AssertEqual(0, CartridgeImportSource.Require(rom).ReadCartridgeWord(0x908b93), "inactive atmospheric timer pointer");
        AssertEqual(0, CartridgeImportSource.Require(rom).ReadCartridgeWord(0x908ba3), "trailing atmospheric timer pointer");
        AssertEqual(0, CartridgeImportSource.Require(rom).ReadCartridgeWord(0x908bef), "inactive atmospheric frame count");

        // Type two, which no producer starts, has no particle kind; types six and seven are both dust, so both are
        // compared with the one dust cadence.
        foreach (int type in StartedEffectNumbers)
        {
            SamusParticleKind kind = KindNumbered(type);
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x908b93 + type * 2));
            int frameCount = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x908bef + type * 2));
            AssertEqual(frameCount, SamusParticleAnimations.StepCount(kind), $"atmospheric type {type} frame count");

            for (int frame = 0; frame < frameCount; frame++)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x900000 | (pointer + frame * 2))),
                    SamusParticleAnimations.StepUpdates(kind, frame),
                    $"atmospheric type {type} frame {frame} timer");
            }
        }

        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x909e8b), SamusLiquidDamageDefinitions.Lava.SubDamage,
            "lava fractional damage rate");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x909e8d), SamusLiquidDamageDefinitions.Lava.WholeDamage,
            "lava whole damage rate");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x909e8f), SamusLiquidDamageDefinitions.Acid.SubDamage,
            "acid fractional damage rate");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x909e91), SamusLiquidDamageDefinitions.Acid.WholeDamage,
            "acid whole damage rate");

        var guarded = NewSamusAtmosphericAnimationReadGuard(rom);
        Suite(nameof(VerifyProductionAtmosphericCadence), () => VerifyProductionAtmosphericCadence());
        Suite(nameof(VerifyParticleLifetimes), () => VerifyParticleLifetimes());
        Suite(nameof(VerifyProductionLiquidDamage), () => VerifyProductionLiquidDamage(RoomFxType.Lava,
            SamusLiquidDamageDefinitions.Lava));
        Suite(nameof(VerifyProductionLiquidDamage), () => VerifyProductionLiquidDamage(RoomFxType.Acid,
            SamusLiquidDamageDefinitions.Acid));

        AssertThrows<InvalidDataException>(
            () => SamusParticleAnimations.StepCount((SamusParticleKind)(-1)),
            "an undefined particle kind has no animation definition");
        AssertThrows<InvalidDataException>(
            () => SamusParticleAnimations.StepUpdates(SamusParticleKind.SurfaceSplash, 4),
            "a step past the surface splash's authored cadence");

        Console.WriteLine(
            "Samus atmospheric animation: 37 timers, seven frame counts, four liquid-damage words, and all real cadence/damage consumers pass with mechanics ranges forbidden.");
    }

    /// <summary>The cartridge's effect numbers that a producer starts: all but zero (an empty place) and two.</summary>
    private static readonly int[] StartedEffectNumbers = [1, 3, 4, 5, 6, 7];

    /// <summary>The particle kind of a started effect number.</summary>
    private static SamusParticleKind KindNumbered(int type) =>
        SamusAtmosphericEffectWordCodec.KindNumbered(type) ?? throw new InvalidDataException($"Effect number {type} names no particle.");

    /// <summary>Checks step expiry and delayed handoff for each particle kind, as each started effect number names it.</summary>
    internal static void VerifyProductionAtmosphericCadence()
    {
        var atmosphericArt = new SamusAtmosphericSourceArtwork(
            new SpritePart[SamusAtmosphericArtworkDefinitions.DirectAtmosphericFrameCount],
            new SpritePart[SamusAtmosphericArtworkDefinitions.DirectAtmosphericFrameCount]);
        foreach (int type in StartedEffectNumbers)
        {
            SamusParticleKind kind = KindNumbered(type);
            int frameCount = SamusParticleAnimations.StepCount(kind);
            for (int frame = 0; frame < frameCount; frame++)
            {
                int expectedTimer = SamusParticleAnimations.StepUpdates(kind, frame);

                // A step with one update left ends on the next update: it takes the ended step's duration and moves on,
                // or the particle finishes and is removed after its last step.
                var particles = new SamusParticles();
                particles.Add(new SamusParticle(kind, frame, SamusParticleStart.Shown(1), new(100, 100), Offset16<RoomPixels>.Zero));
                var oamFrame = new SpriteFrameBuffer();
                oamFrame.BeginFrame();
                particles.UpdateAndDraw(oamFrame, Position16<RoomPixels>.Zero, waterSurfaceLine: 100,
                    SamusBodySheetFixtures.ForAtmosphere(atmosphere: atmosphericArt));
                oamFrame.FinalizeAsOam();
                if (frame + 1 == frameCount)
                {
                    AssertEqual(0, particles.All.Count, $"production atmospheric type {type} frame {frame} finishes");
                }
                else
                {
                    AssertEqual(expectedTimer, particles.All[0].StepUpdatesLeft,
                        $"production atmospheric type {type} frame {frame} expiry timer");
                    AssertEqual(frame + 1, particles.All[0].AnimationStep,
                        $"production atmospheric type {type} frame {frame} expiry handoff");
                }

                // A particle hidden for one update shows on it, with its step's full duration.
                particles.Clear();
                particles.Add(new SamusParticle(kind, frame, SamusParticleStart.Hidden(1), new(100, 100), Offset16<RoomPixels>.Zero));
                oamFrame.BeginFrame();
                particles.UpdateAndDraw(oamFrame, Position16<RoomPixels>.Zero, waterSurfaceLine: 100,
                    SamusBodySheetFixtures.ForAtmosphere(atmosphere: atmosphericArt));
                oamFrame.FinalizeAsOam();
                AssertEqual(expectedTimer, particles.All[0].StepUpdatesLeft,
                    $"production atmospheric type {type} frame {frame} delayed timer");
                AssertEqual(frame, particles.All[0].AnimationStep,
                    $"production atmospheric type {type} frame {frame} delayed handoff");
                AssertTrue(!particles.All[0].IsHidden, $"production atmospheric type {type} frame {frame} shows");
            }
        }
    }

    /// <summary>
    /// Checks that the updates a particle is shown, which the breath bubble and lava spray waits last (#1556), are the
    /// updates its animation runs before it is removed, for every kind, step and the starts the producers and native
    /// timers use.
    /// </summary>
    private static void VerifyParticleLifetimes()
    {
        SamusParticleStart[] starts =
        [
            SamusParticleStart.Shown(1), SamusParticleStart.Shown(2), SamusParticleStart.Shown(3), SamusParticleStart.Shown(9),
            SamusParticleStart.Hidden(1), SamusParticleStart.Hidden(2), SamusParticleStart.Hidden(5),
        ];
        foreach (SamusParticleKind kind in Enum.GetValues<SamusParticleKind>())
        {
            for (int step = 0; step < SamusParticleAnimations.StepCount(kind); step++)
            {
                foreach (SamusParticleStart start in starts)
                {
                    var particle = new SamusParticle(kind, step, start, new(100, 100), Offset16<RoomPixels>.Zero);
                    int updates = 0;
                    while (!particle.IsFinished)
                    {
                        particle.Advance();
                        updates++;
                    }
                    AssertEqual(updates, SamusParticleAnimations.UpdatesUntilFinished(kind, step, start),
                        $"{kind} step {step} {start} is shown for the updates its wait lasts");
                }
            }
        }
        // The producers' own starts, worked from the cadence: a bubble holds the next for 38 updates, a landing's
        // dust for 15, a lava spray's first particle the next spray for 9 and its third the bubbles for 10, and a
        // boosted footstep's leading dust the spray for 17.
        AssertEqual(38, SamusParticleAnimations.UpdatesUntilFinished(SamusParticleKind.Bubbles, 0, SamusParticleStart.Shown(3)), "bubble wait");
        AssertEqual(15, SamusParticleAnimations.UpdatesUntilFinished(SamusParticleKind.Dust, 0, SamusParticleStart.Shown(3)), "landing dust wait");
        AssertEqual(9, SamusParticleAnimations.UpdatesUntilFinished(SamusParticleKind.LavaSpray, 0, SamusParticleStart.Shown(3)), "spray wait");
        AssertEqual(10, SamusParticleAnimations.UpdatesUntilFinished(SamusParticleKind.LavaSpray, 0, SamusParticleStart.Hidden(2)), "spray bubble wait");
        AssertEqual(17, SamusParticleAnimations.UpdatesUntilFinished(SamusParticleKind.Dust, 0, SamusParticleStart.Hidden(2)), "boosted dust wait");
    }

    /// <summary>Checks the periodic whole and fractional damage installed for submerged Samus in lava or acid.</summary>
    private static void VerifyProductionLiquidDamage(
        RoomFxType fxType,
        SamusLiquidDamageRate expected)
    {
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Health = 99,
            Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
            Kinematics = { Radius = new(5, 12) },
        };
        samus.LiquidPhysics.ConfigureLavaAcid(
            surfaceY: 100,
            fxType == RoomFxType.Acid ? RoomFxType.Acid : RoomFxType.Lava);
        samus.LiquidPhysics.PrepareAnimationFrame(samus, new SharedRandom(), frameCounter: 1);
        AssertEqual(expected.SubDamage, samus.LiquidPhysics.PendingPeriodicDamage.Fraction,
            $"production {fxType} fractional damage accumulation");
        AssertEqual(expected.WholeDamage, samus.LiquidPhysics.PendingPeriodicDamage.Whole,
            $"production {fxType} whole damage accumulation");
    }

    /// <summary>Rejects runtime reads of compiled atmosphere timing and liquid-damage table bytes.</summary>
    internal static MutableCartridgeReadGuard NewSamusAtmosphericAnimationReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= 0x908b93 and < 0x908bff or >= 0x909e8b and < 0x909e93,
            address => $"Samus atmosphere attempted migrated mechanics read ${address:X6}.");
        return guard;
    }

    /// <summary>Runs atmospheric cadence and liquid-damage checks with the repository cartridge.</summary>
    [VerificationCommand("--samus-atmospheric-cadence")]
    internal static int SamusAtmosphericCadenceCommand()
    {
        string atmosphereCadenceRom = RepositoryRomPath;
        Suite(nameof(VerifySamusAtmosphericAnimationDefinitions), () => VerifySamusAtmosphericAnimationDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(atmosphereCadenceRom)));
        return 0;
    }
}
