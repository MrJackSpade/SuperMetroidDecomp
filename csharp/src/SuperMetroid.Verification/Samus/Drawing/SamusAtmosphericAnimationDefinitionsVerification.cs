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
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Artwork;

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
            AssertEqual(frameCount, PictureCount(kind), $"atmospheric type {type} frame count");

            // Each picture shows for the native timer of the step before it, the first for its own (#2388); the last step's
            // own timer is never shown.
            for (int frame = 0; frame < frameCount; frame++)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x900000 | (pointer + Math.Max(frame - 1, 0) * 2))),
                    PictureUpdates(kind, frame),
                    $"atmospheric type {type} picture {frame} shows for its handed-on timer");
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

        Console.WriteLine(
            "Samus atmospheric animation: 37 timers, seven frame counts, four liquid-damage words, and all real cadence/damage consumers pass with mechanics ranges forbidden.");
    }

    /// <summary>The cartridge's effect numbers that a producer starts: all but zero (an empty place) and two.</summary>
    private static readonly int[] StartedEffectNumbers = [1, 3, 4, 5, 6, 7];

    /// <summary>The pictures <paramref name="kind"/>'s animation shows.</summary>
    internal static int PictureCount(SamusParticleKind kind) => SamusParticleAnimations.Segments.Beats(kind).Count;

    /// <summary>The updates picture <paramref name="picture"/> of <paramref name="kind"/>'s animation shows for.</summary>
    internal static int PictureUpdates(SamusParticleKind kind, int picture) =>
        ((AnimationBeat<SamusAtmosphereFrame, NoAnimationCue>.Frame)SamusParticleAnimations.Segments.Beats(kind)[picture]).Duration;

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
            int frameCount = PictureCount(kind);
            for (int frame = 0; frame < frameCount; frame++)
            {
                // A picture with one update left ends on the next update: the next picture shows for its own updates, or
                // the particle finishes and is removed after its last picture.
                var particles = new SamusParticles();
                particles.Add(SamusParticle.AtStep(kind, frame, SamusParticleStart.Shown(1), new(100, 100), Offset16<RoomPixels>.Zero));
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
                    AssertEqual(PictureUpdates(kind, frame + 1), particles.All[0].StepUpdatesLeft,
                        $"production atmospheric type {type} frame {frame} expiry timer");
                    AssertEqual(frame + 1, particles.All[0].AnimationStep,
                        $"production atmospheric type {type} frame {frame} expiry handoff");
                }
            }

            // A particle hidden for one update shows its first picture on it, for that picture's updates; every producer
            // starts a hidden particle on its first picture.
            var hidden = new SamusParticles();
            hidden.Add(SamusParticle.AtStep(kind, 0, SamusParticleStart.Hidden(1), new(100, 100), Offset16<RoomPixels>.Zero));
            var hiddenFrame = new SpriteFrameBuffer();
            hiddenFrame.BeginFrame();
            hidden.UpdateAndDraw(hiddenFrame, Position16<RoomPixels>.Zero, waterSurfaceLine: 100,
                SamusBodySheetFixtures.ForAtmosphere(atmosphere: atmosphericArt));
            hiddenFrame.FinalizeAsOam();
            AssertEqual(PictureUpdates(kind, 0), hidden.All[0].StepUpdatesLeft, $"production atmospheric type {type} delayed timer");
            AssertEqual(0, hidden.All[0].AnimationStep, $"production atmospheric type {type} delayed handoff");
            AssertTrue(!hidden.All[0].IsHidden, $"production atmospheric type {type} shows");
            AssertThrows<ArgumentOutOfRangeException>(
                () => SamusParticle.AtStep(kind, 1, SamusParticleStart.Hidden(1), new(100, 100), Offset16<RoomPixels>.Zero),
                $"atmospheric type {type} starts hidden only on its first picture");
        }
    }

    /// <summary>
    /// Checks the updates a particle is shown, which the breath bubble and lava spray waits last (#1556), for the starts
    /// the producers use, worked from the native cadence (#2388).
    /// </summary>
    private static void VerifyParticleLifetimes()
    {
        // The producers' own starts, worked from the cadence: a bubble holds the next for 38 updates, a landing's
        // dust for 15, a lava spray's first particle the next spray for 9 and its third the bubbles for 10, and a
        // boosted footstep's leading dust the spray for 17.
        AssertEqual(38, SamusParticle.UpdatesUntilFinished(SamusParticleKind.Bubbles, AnimationPlace.StartOfAnimation, SamusParticleStart.Shown(3)), "bubble wait");
        AssertEqual(15, SamusParticle.UpdatesUntilFinished(SamusParticleKind.Dust, AnimationPlace.StartOfAnimation, SamusParticleStart.Shown(3)), "landing dust wait");
        AssertEqual(9, SamusParticle.UpdatesUntilFinished(SamusParticleKind.LavaSpray, AnimationPlace.StartOfAnimation, SamusParticleStart.Shown(3)), "spray wait");
        AssertEqual(10, SamusParticle.UpdatesUntilFinished(SamusParticleKind.LavaSpray, AnimationPlace.StartOfAnimation, SamusParticleStart.Hidden(2)), "spray bubble wait");
        AssertEqual(17, SamusParticle.UpdatesUntilFinished(SamusParticleKind.Dust, AnimationPlace.StartOfAnimation, SamusParticleStart.Hidden(2)), "boosted dust wait");
    }

    /// <summary>Checks the periodic whole and fractional damage installed for submerged Samus in lava or acid.</summary>
    private static void VerifyProductionLiquidDamage(
        RoomFxType fxType,
        SamusLiquidDamageRate expected)
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Health = 99,
            Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
            Kinematics = { Radius = new(5, 12) },
        };
        samus.LiquidPhysics.ConfigureLavaAcid(
            surfaceY: 100,
            fxType == RoomFxType.Acid ? RoomFxType.Acid : RoomFxType.Lava);
        samus.LiquidPhysics.PrepareAnimationFrame(samus, new SharedRandom(), frameCounter: 1, bossPresent: false);
        AssertEqual(expected.SubDamage, samus.PeriodicDamage.Pending.Fraction,
            $"production {fxType} fractional damage accumulation");
        AssertEqual(expected.WholeDamage, samus.PeriodicDamage.Pending.Whole,
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
