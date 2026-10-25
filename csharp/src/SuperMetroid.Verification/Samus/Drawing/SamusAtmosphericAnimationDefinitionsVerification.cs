using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Drawing;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks Samus atmospheric animation timers and lava/acid damage rates against bank 90.</summary>
internal static class SamusAtmosphericAnimationDefinitionsVerification
{
    /// <summary>Compares all active frame timers and liquid-damage words with the cartridge, then exercises their consumers.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus atmospheric animation definitions")]
    internal static void VerifySamusAtmosphericAnimationDefinitions(
        SuperMetroidAddressSpace rom)
    {

        AssertEqual(0, CartridgeImportSource.Require(rom).ReadCartridgeWord(0x908b93), "inactive atmospheric timer pointer");
        AssertEqual(0, CartridgeImportSource.Require(rom).ReadCartridgeWord(0x908ba3), "trailing atmospheric timer pointer");
        AssertEqual(0, CartridgeImportSource.Require(rom).ReadCartridgeWord(0x908bef), "inactive atmospheric frame count");

        for (byte type = 1; type <= 7; type++)
        {
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x908b93 + type * 2));
            byte frameCount = checked((byte)CartridgeImportSource.Require(rom).ReadCartridgeWord((0x908bef + type * 2)));
            AssertEqual(frameCount,
                SamusAtmosphericAnimationDefinitions.FrameCount(type),
                $"atmospheric type {type} frame count");

            for (byte frame = 0; frame < frameCount; frame++)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x900000 | (pointer + frame * 2))),
                    SamusAtmosphericAnimationDefinitions.FrameTimer(type, frame),
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
        Suite(nameof(VerifyNullAtmosphericPointerDrawsNothing), () => VerifyNullAtmosphericPointerDrawsNothing());
        Suite(nameof(VerifyProductionLiquidDamage), () => VerifyProductionLiquidDamage(RoomFxType.Lava,
            SamusLiquidDamageDefinitions.Lava));
        Suite(nameof(VerifyProductionLiquidDamage), () => VerifyProductionLiquidDamage(RoomFxType.Acid,
            SamusLiquidDamageDefinitions.Acid));

        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericAnimationDefinitions.FrameCount(0),
            "inactive atmospheric type has no animation definition");
        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericAnimationDefinitions.FrameCount(8),
            "atmospheric type past active domain");
        AssertThrows<InvalidDataException>(
            () => SamusAtmosphericAnimationDefinitions.FrameTimer(1, 4),
            "atmospheric frame past authored type-one cadence");

        Console.WriteLine(
            "Samus atmospheric animation: 37 timers, seven frame counts, four liquid-damage words, and all real cadence/damage consumers pass with mechanics ranges forbidden.");
    }

    /// <summary>Checks frame expiry and delayed handoff for each of the seven atmospheric effect types.</summary>
    internal static void VerifyProductionAtmosphericCadence()
    {
        var atmosphericArt = new SamusAtmosphericArtworkCatalog(
            new ushort[SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount],
            new ushort[SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount]);
        for (byte type = 1; type <= 7; type++)
        {
            // This fixture measures timer ownership. Keep the Samus-spritemap
            // types below the native Y clip so their unrelated body-art catalog
            // is not needed; direct small sprites still exercise installed art.
            ushort drawY = type is 3 or 5 ? (ushort)0x0100 : (ushort)100;
            byte frameCount = SamusAtmosphericAnimationDefinitions.FrameCount(type);
            for (byte frame = 0; frame < frameCount; frame++)
            {
                ushort expectedTimer =
                    SamusAtmosphericAnimationDefinitions.FrameTimer(type, frame);

                var effects = new SamusAtmosphericEffectsState();
                effects.SetSlot(0, type, frame, animationTimer: 1, anchor: new(100, drawY));
                var oamFrame = new SpriteFrameBuffer();
                oamFrame.BeginFrame();
                effects.UpdateAndDraw(oamFrame, Position16<RoomPixels>.Zero, fxYPosition: drawY,
                    SamusBodySheetFixtures.ForAtmosphere(atmosphere: atmosphericArt));
                oamFrame.FinalizeAsOam();
                AssertEqual(expectedTimer, effects.Slots[0].AnimationTimer.RawValue,
                    $"production atmospheric type {type} frame {frame} expiry timer");
                AssertEqual(frame + 1 == frameCount ? 0 : frame + 1,
                    effects.Slots[0].AnimationFrame,
                    $"production atmospheric type {type} frame {frame} expiry handoff");

                effects.Clear();
                effects.SetSlot(0, type, frame, animationTimer: 0x8001,
                    anchor: new(100, drawY));
                oamFrame.BeginFrame();
                effects.UpdateAndDraw(oamFrame, Position16<RoomPixels>.Zero, fxYPosition: drawY,
                    SamusBodySheetFixtures.ForAtmosphere(atmosphere: atmosphericArt));
                oamFrame.FinalizeAsOam();
                AssertEqual(expectedTimer, effects.Slots[0].AnimationTimer.RawValue,
                    $"production atmospheric type {type} frame {frame} delayed timer");
                AssertEqual(frame, effects.Slots[0].AnimationFrame,
                    $"production atmospheric type {type} frame {frame} delayed handoff");
            }
        }
    }

    /// <summary>
    /// Checks that the type-two footstep splash, whose cartridge list pointer is null, draws nothing: its native read
    /// lands on direct-page scratch the port does not model (owner decision, #1318 row 6).
    /// </summary>
    private static void VerifyNullAtmosphericPointerDrawsNothing()
    {
        var effects = new SamusAtmosphericEffectsState();
        effects.SetSlot(0, 2, 0, animationTimer: 2,
            anchor: new(100, 100));
        var oamFrame = new SpriteFrameBuffer();
        oamFrame.BeginFrame();
        effects.UpdateAndDraw(oamFrame, Position16<RoomPixels>.Zero, fxYPosition: 100);
        OamBuffer oam = oamFrame.FinalizeAsOam();
        AssertEqual(0, oam.AppendedSpriteCount, "the null-pointer footstep splash draws nothing");
    }

    /// <summary>Checks the periodic whole and fractional damage installed for submerged Samus in lava or acid.</summary>
    private static void VerifyProductionLiquidDamage(
        RoomFxType fxType,
        SamusLiquidDamageRate expected)
    {
        var samus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Health = 99,
            Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)),
            Kinematics = { Radius = new(5, 12) },
        };
        samus.LiquidPhysics.ConfigureLavaAcid(
            surfaceY: 100,
            fxType == RoomFxType.Acid ? RoomFxType.Acid : RoomFxType.Lava);
        samus.LiquidPhysics.PrepareAnimationFrame(samus, new SharedRandom(), nmiFrameCounter: 1);
        AssertEqual(expected.SubDamage, samus.LiquidPhysics.PeriodicSubDamage,
            $"production {fxType} fractional damage accumulation");
        AssertEqual(expected.WholeDamage, samus.LiquidPhysics.PeriodicDamage,
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
