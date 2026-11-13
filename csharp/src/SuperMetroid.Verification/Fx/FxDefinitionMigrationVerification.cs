using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.Verification.Fx;

/// <summary>
/// Confirms #1415's room-effect migration: each deleted Fx RomData catalog's values are still produced, for one member
/// of each kind, by the domain catalogs that replace it; the native-word tables translate the identities whose values
/// became arbitrary; and the import's source mappings name the addresses the atlas and the effect pages were keyed by
/// before. None reads the ROM.
/// </summary>
internal static class FxDefinitionMigrationVerification
{
    /// <summary>Compares the new definitions, translations and import sources with the values the deleted catalogs held.</summary>
    internal static void VerifyFxDefinitionMigration()
    {
        // The identities' values are arbitrary; their cartridge words are verification's.
        AssertEqual(0x82ab, FxNativeWords.AnimatedTileObjects.Word(AnimatedTileObject.Lava), "lava object header word");
        AssertEqual(AnimatedTileObject.WreckedShipTreadmillRightwards, FxNativeWords.AnimatedTileObjects.Decode(0x8275, "object"),
            "rightward treadmill header decodes");
        AssertEqual(0xb4b6, SpecialAirReactionNativeWords.InsideSetups[SpecialAirInsideReaction.FastSandfall], "fast sandfall setup word");
        AssertEqual(0xe440, FxNativeWords.PaletteFxSetups.Word(NativePaletteFxSetup.Norfair), "Norfair palette setup word");
        AssertEqual(ColourEffectStart.ChoosesHeatAnimationForSuit, FxNativeWords.StartOf(NativePaletteFxSetup.Norfair), "Norfair palette setup start");
        AssertEqual(0xe379, FxNativeWords.PaletteFxPreInstructions.Word(NativePaletteFxPreInstruction.Heat), "heat pre-instruction word");
        AssertEqual(ColourEffectCondition.BurnsSamusInHeat.Instance, FxNativeWords.ConditionOf(NativePaletteFxPreInstruction.Heat), "heat pre-instruction condition");

        // QuicksandRomData and ConveyorBlockRomData's displacements, now in sixteenths of a pixel.
        AssertEqual(0x12000, QuicksandDefinitions.SubmergingSinkDisplacement.RawValue, "submerging sand sink");
        AssertEqual(0x14000, QuicksandDefinitions.SlowSandfallDisplacement.RawValue, "slow sandfall");
        AssertEqual(0x1c000, QuicksandDefinitions.FastSandfallDisplacement.RawValue, "fast sandfall");
        AssertEqual(0x3000, QuicksandDefinitions.StationarySurfaceProbeLimit.RawValue, "stationary surface probe limit");
        AssertEqual(0x28000, QuicksandReactions.SurfacePhysics(gravitySuit: false).UpwardSpeedLimit.RawValue, "upward cap without Gravity");
        AssertEqual(0x10000, QuicksandReactions.SurfacePhysics(gravitySuit: true).StationaryDisplacement.RawValue, "stationary sink with Gravity");
        AssertEqual(2, ConveyorBlockDefinitions.RightwardCarry, "rightward conveyor carry");
        AssertEqual(-2, ConveyorBlockDefinitions.LeftwardCarry, "leftward conveyor carry");

        // The Fireflea shades are fade levels: the high bytes of the former shade words, below their channel bits.
        AssertEqual(2, FirefleaShades.DarknessShade(6), "the sixth death's faint darkness level");
        AssertEqual(25, FirefleaShades.DarknessShade(5), "deepest darkness level");
        AssertEqual(6, FirefleaShades.FlashingShade(6), "brightest flash level");

        // The atlas is keyed by picture; the import reads each picture from the address it was keyed by.
        AssertEqual(34, RoomFxAnimatedTileAtlasFile.Segments.Count(), "atlas segments");
        AssertEqual(0x8791e4, RoomFxAnimatedTileSources.Of(RoomFxAnimatedTileAtlasFile.Segments.First()).Value, "first atlas segment source");
        AssertEqual(0x879e84, RoomFxAnimatedTileSources.Of(new AnimatedTileImage(AnimatedTileObject.HorizontalSpikes, 2)).Value, "third spike picture source");
        AssertEqual(0x879364, RoomFxAnimatedTileSources.Of(new RoomFxAtlasSegment.TourianStatueStrip()).Value, "statue strip source");
        AssertEqual(0x878ec4, AnimatedTileImageSources.Treadmill(3), "fourth treadmill picture source");
        AssertEqual(0x8798e4, AnimatedTileImageSources.TourianStatue(SuperMetroid.Core.Areas.Tourian.TourianGreyStatues.Draygon, TourianStatueTilePicture.Released),
            "Draygon released statue picture source");
        AssertEqual(0x40, RoomFxAnimatedTilePictures.TourianStatuePictureByteCount(SuperMetroid.Core.Areas.Tourian.TourianGreyStatues.Ridley),
            "Ridley statue picture size");
        RoomFxAnimatedTileLoop spikes = RoomFxAnimatedTileLoops.Require(AnimatedTileObject.HorizontalSpikes);
        AssertEqual(new AnimatedTileImage(AnimatedTileObject.HorizontalSpikes, 1), spikes.Image(3), "the fourth spike step repeats the second picture");
        AssertEqual(new AnimatedTileImage(AnimatedTileObject.WreckedShipTreadmillRightwards, 3),
            WreckedShipTreadmills.ForDirection(WreckedShipTreadmillDirection.Leftwards).Image(0),
            "the leftward treadmill starts on the rightward loop's last picture");
        AssertEqual(0x20, RoomFxAnimatedTileLoops.TreadmillFrameByteCount, "treadmill frame size");
        AssertEqual(0x40, RoomFxAnimatedTileLoops.Require(AnimatedTileObject.Lava).PictureByteCount, "liquid frame size");
        AssertEqual(0x50, RoomFxAnimatedTileLoops.Require(AnimatedTileObject.Rain).PictureByteCount, "rain frame size");

        // RoomFxRomData's effect motion, waves and tides.
        AssertEqual(10, WaterWaveDefinitions.OverlayWavePhaseDuration, "water layer-three wave step");
        AssertEqual(0x40, WaterWaveDefinitions.HorizontalSubscrollVelocity, "water drift");
        AssertEqual(16, WaterWaveDefinitions.WaveDisplacementCount, "water wave steps");
        AssertEqual(-1, LiquidEffects.WaterWaveDisplacement(5), "water wave sample 5");
        AssertEqual(1, LiquidEffects.HeatHorizontalWaveDisplacement(2), "horizontal heat-haze sample 2");
        AssertEqual(-1, LiquidEffects.HeatHorizontalWaveDisplacement(8), "horizontal heat-haze sample 8");
        AssertEqual(4, LavaAcidWaveDefinitions.VerticalWavePhaseDuration, "vertical heat-haze step");
        AssertEqual(0x70, LavaAcidWaveDefinitions.AmbientSoundPeriod, "lava sound period");
        AssertEqual(288, LiquidTideDefinitions.SmallTidePositivePhaseDelta.RawValue, "small tide rising phase step");
        AssertEqual(0x0600, AtmosphereDriftDefinitions.RainVerticalVelocity, "rain fall");
        AssertEqual(-6 * 256, AtmosphereDrift.RainHorizontalVelocity(RainWind.StrongLeft), "strong left rain wind: six pixels left, in 1/256 pixel");
        AssertEqual(4 * 256, AtmosphereDrift.RainHorizontalVelocity(RainWind.GentleRight), "gentle right rain wind: four pixels right, in 1/256 pixel");
        AssertEqual(0x0040, AtmosphereDriftDefinitions.SporesVerticalVelocity, "spore rise");
        AssertEqual(0x0050, AtmosphereDriftDefinitions.FogHorizontalVelocity, "fog drift");

        // The sky's bands (#1687): the cartridge's 23 entries, the bottom three still ones as one band.
        AssertEqual(21, ScrollingSkyBandDefinitions.Bands.Length, "sky bands");
        AssertEqual(0x0500, ScrollingSkyBandDefinitions.EndRow, "sky end row");
        AssertEqual(0x02e0, ScrollingSkyBandDefinitions.Bands[11].TopRow, "band 11 top");
        AssertTrue(ScrollingSkyBandDefinitions.Bands[11].Drift == ScrollingSkyBandDefinitions.Bands[8].Drift,
            "bands 8 and 11 share one drift");
        AssertEqual(0x14000, ScrollingSkyBandDefinitions.Bands[11].Drift.Speed.RawValue, "band 11 drift: both entries' speeds");
        AssertEqual(0x7000, ScrollingSkyBandDefinitions.Bands[17].Drift.Speed.RawValue, "band 17 drift");
        AssertEqual(0x8000, ScrollingSkyBandDefinitions.Bands[19].Drift.Speed.RawValue, "band 19 drift");
        AssertEqual(0x0490, ScrollingSkyBandDefinitions.Bands[^1].TopRow, "the still bottom band's top");
        AssertEqual(0, ScrollingSkyBandDefinitions.Bands[^1].Drift.Speed.RawValue, "the bottom band stands still");

        // RoomFxRomData's earthquake rules.
        AssertTrue(!NativeEarthquakeTypes.Shake(0x0011).ShakesEnemies && NativeEarthquakeTypes.Shake(0x0012).ShakesEnemies,
            "first enemy-shaking earthquake");
        AssertEqual(0x0015, NativeEarthquakeTypes.TypeOf(RoomEarthquakeDefinitions.RisingLiquidShake), "rising liquid earthquake");
        AssertEqual(0x0020, RoomEarthquakeDefinitions.SustainedShakeFrames, "rising liquid timer bits");
        AssertEqual(6, RoomEarthquakeDefinitions.SoundSuppressedRooms.Count, "silent quake rooms");
        AssertEqual(RoomHeaderDefinitions.BombTorizo, RoomEarthquakeDefinitions.SoundSuppressedRooms[0], "first silent quake room");

        // RoomFxRomData's layer-three values, and the import's pages and blends.
        AssertEqual(0x184e, FxOverlayPlaneConstants.ClearedCell.PackedWord, "cleared effect cell");
        AssertEqual(3, RoomFxPaletteBlendDefinitions.ColorCount, "palette blend colours");
        AssertEqual(0x8a8000 + 5 * 0x840, RoomFxLayer3TilemapSources.Page(RoomFxType.Fog).Value, "fog page source");
        AssertEqual(RoomFxLayer3TilemapSources.Page(RoomFxType.Water), RoomFxLayer3TilemapSources.Page(RoomFxType.TourianEntranceStatue),
            "the statue effect shows the water page");
        AssertEqual(0x89aa02 + 0x62, RoomFxLayer3TilemapSources.PaletteBlend(RoomFxPaletteBlend.Fog).Value, "fog blend source");
        // The FX records hold decoded fields; the record codec encodes them back to the bytes the catalog held before.
        RoomFxRecordDefinition climb = RoomFxDefinitions.ClimbWhenZebesTimebombSet.Records[0];
        AssertEqual(RoomFxType.Acid, climb.Type, "Climb FX type");
        AssertEqual(RoomEffectBlendDefinitions.OverlayBrightens, climb.EffectBlend, "Climb effect blend");
        AssertEqual(RoomLiquidOptions.SurfaceDrift, climb.LiquidOptions, "Climb liquid options");
        AssertEqual("OldTourianRedFlashing, OldTourianBackgroundRailingsFlashYellow, OldTourianBackgroundPanelsFlashYellow",
            string.Join(", ", climb.PaletteEffects), "Climb palette effects");
        AssertEqual(AreaId.Crateria, RoomFxHeaderAreas.Of(RoomFxDefinitions.ClimbWhenZebesTimebombSet), "Climb FX area");
        AssertEqual((byte)0x04, RoomFxRecordEncoding.TypeByte(climb.Type), "Climb type byte");
        AssertEqual((byte)0x38, RoomFxRecordEncoding.PaletteEffectsByte(climb.PaletteEffects, AreaId.Crateria), "Climb palette-effect byte");
        AssertEqual(0x2c, SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word(RoomFxType.CeresHaze), "Ceres haze type byte");
        AssertEqual(0xee, SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.PaletteBlends.Word(RoomFxPaletteBlend.MaridiaWaterD), "Maridia water D blend byte");
        AssertEqual("blend-EE", RoomFxPaletteBlendDocumentRules.Key(RoomFxPaletteBlend.MaridiaWaterD), "installed blend key keeps its name");
        AssertEqual((byte)0x80, RoomFxRecordEncoding.PaletteEffectsByte([PaletteFxObject.BeaconFlashing], AreaId.Crateria),
            "Crateria's eighth palette effect");
        Console.WriteLine("Fx definition migration: native words, catalog values, atlas picture sources, effect page sources and decoded FX records keep their values.");
    }

    /// <summary>Runs the focused room-effect definition-migration check.</summary>
    [VerificationCommand("--fx-definition-migration")]
    internal static int FxDefinitionMigrationCommand()
    {
        VerifyFxDefinitionMigration();
        return 0;
    }
}
