using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Verification.Harness;

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
        AssertEqual(0xb4b6, FxNativeWords.SpecialAirReactionSetups.Word(SpecialAirReactionSetup.SandFallsFast), "fast sandfall setup word");
        AssertEqual(0xe440, FxNativeWords.PaletteFxSetups.Word(PaletteFxSetup.Norfair), "Norfair palette setup word");
        AssertEqual(0xe379, FxNativeWords.PaletteFxPreInstructions.Word(PaletteFxPreInstruction.Heat), "heat pre-instruction word");

        // QuicksandRomData and ConveyorBlockRomData's displacements, now in sixteenths of a pixel.
        AssertEqual(0x12000, QuicksandDefinitions.SubmergingSinkDisplacement.RawValue, "submerging sand sink");
        AssertEqual(0x14000, QuicksandDefinitions.SlowSandfallDisplacement.RawValue, "slow sandfall");
        AssertEqual(0x1c000, QuicksandDefinitions.FastSandfallDisplacement.RawValue, "fast sandfall");
        AssertEqual(0x3000, QuicksandDefinitions.StationarySurfaceProbeLimit.RawValue, "stationary surface probe limit");
        AssertEqual(0x28000, QuicksandReactions.SurfacePhysics(gravitySuit: false).UpwardSpeedLimit.RawValue, "upward cap without Gravity");
        AssertEqual(0x10000, QuicksandReactions.SurfacePhysics(gravitySuit: true).StationaryDisplacement.RawValue, "stationary sink with Gravity");
        AssertEqual((short)2, ConveyorBlockDefinitions.RightwardCarry, "rightward conveyor carry");
        AssertEqual((short)-2, ConveyorBlockDefinitions.LeftwardCarry, "leftward conveyor carry");

        // The Fireflea shades are the high bytes of the former shade words.
        AssertEqual((byte)0xc2, FirefleaShades.DarknessShade(12), "darkness level past the ramp");
        AssertEqual((byte)25, FirefleaShades.DarknessShade(10), "last ramp darkness level");
        AssertEqual((byte)6, FirefleaShades.FlashingShade(6), "brightest flash level");

        // The atlas is keyed by picture; the import reads each picture from the address it was keyed by.
        AssertEqual(34, RoomFxAnimatedTileAtlasFormat.Segments.Count(), "atlas segments");
        AssertEqual(0x8791e4, RoomFxAnimatedTileSources.Of(RoomFxAnimatedTileAtlasFormat.Segments.First()).Value, "first atlas segment source");
        AssertEqual(0x879e84, RoomFxAnimatedTileSources.Of(new AnimatedTileImage(AnimatedTileObject.HorizontalSpikes, 2)).Value, "third spike picture source");
        AssertEqual(0x879364, RoomFxAnimatedTileSources.Of(new RoomFxAtlasSegment.TourianStatueStrip()).Value, "statue strip source");
        AssertEqual(0x878ec4, AnimatedTileImageSources.Treadmill(3), "fourth treadmill picture source");
        AssertEqual(0x8798e4, AnimatedTileImageSources.TourianStatue(AnimatedTileObject.TourianStatueDraygon, TourianStatueTilePicture.Released),
            "Draygon released statue picture source");
        AssertEqual(0x40, TourianStatueTileStrip.WindowByteCount(AnimatedTileObject.TourianStatueRidley), "Ridley statue picture size");
        RoomFxAnimatedTileLoop spikes = RoomFxAnimatedTiles.Require(AnimatedTileObject.HorizontalSpikes);
        AssertEqual(new AnimatedTileImage(AnimatedTileObject.HorizontalSpikes, 1), spikes.Image(spikes.Frames[3]), "the fourth spike step repeats the second picture");
        AssertEqual(new AnimatedTileImage(AnimatedTileObject.WreckedShipTreadmillRightwards, 3),
            WreckedShipTreadmills.ForDirection(WreckedShipTreadmillDirection.Leftwards).Image(0),
            "the leftward treadmill starts on the rightward loop's last picture");
        AssertEqual(0x20, WreckedShipTreadmillMechanicsDefinitions.FrameByteCount, "treadmill frame size");
        AssertEqual(0x40, RoomFxAnimatedTiles.Require(AnimatedTileObject.Lava).FrameByteCount, "liquid frame size");
        AssertEqual(0x50, RoomFxAnimatedTiles.Require(AnimatedTileObject.Rain).FrameByteCount, "rain frame size");

        // RoomFxRomData's effect motion, waves and tides.
        AssertEqual(10, WaterWaveDefinitions.Bg3WavePhaseDuration, "water layer-three wave step");
        AssertEqual(0x40, WaterWaveDefinitions.HorizontalSubscrollVelocity, "water drift");
        AssertEqual(16, WaterWaveDefinitions.WaveDisplacementCount, "water wave steps");
        AssertEqual((short)-1, LiquidEffects.WaterWaveDisplacement(5), "water wave sample 5");
        AssertEqual((short)1, LiquidEffects.HeatHorizontalWaveDisplacement(2), "horizontal heat-haze sample 2");
        AssertEqual((short)-1, LiquidEffects.HeatHorizontalWaveDisplacement(8), "horizontal heat-haze sample 8");
        AssertEqual(4, LavaAcidWaveDefinitions.VerticalWavePhaseDuration, "vertical heat-haze step");
        AssertEqual(0x70, LavaAcidWaveDefinitions.AmbientSoundPeriod, "lava sound period");
        AssertEqual(288, LiquidTideDefinitions.SmallTidePositivePhaseDelta, "small tide rising phase step");
        AssertEqual(0x0600, AtmosphereDriftDefinitions.RainVerticalVelocity, "rain fall");
        AssertEqual(0xfa00, AtmosphereDrift.RainHorizontalVelocity(0), "rain drift selection 0");
        AssertEqual(0x0400, AtmosphereDrift.RainHorizontalVelocity(3), "rain drift selection 3");
        AssertEqual(0x0040, AtmosphereDriftDefinitions.SporesVerticalVelocity, "spore rise");
        AssertEqual(0x0050, AtmosphereDriftDefinitions.FogHorizontalVelocity, "fog drift");

        // RoomFxRomData's sky bands.
        AssertEqual(23, ScrollingSkyBandDefinitions.SectionCount, "sky bands");
        AssertEqual(0x0500, ScrollingSkyBandDefinitions.WorldEndPosition, "sky world end");
        AssertEqual(0x02e0, ScrollingSkyBands.GetSection(11).TopPosition, "band 11 top");
        AssertEqual(0xc000, ScrollingSkyBands.GetSection(11).Velocity.RawValue, "band 11 drift");
        AssertEqual(8, ScrollingSkyBands.GetSection(11).DataSlot, "band 11 slot");
        AssertEqual(0x7000, ScrollingSkyBands.GetSection(17).Velocity.RawValue, "band 17 drift");
        AssertEqual(0x8000, ScrollingSkyBands.GetSection(19).Velocity.RawValue, "band 19 drift");
        AssertEqual(0x04b8, ScrollingSkyBands.GetSection(22).TopPosition, "last band top");

        // RoomFxRomData's earthquake rules.
        AssertEqual(0x0012, RoomEarthquakeDefinitions.FirstEnemyShakingType, "first enemy-shaking earthquake");
        AssertEqual(0x0024, RoomEarthquakeDefinitions.FirstNonRenderedType, "first unshown earthquake");
        AssertEqual(0x0015, RoomEarthquakeDefinitions.RisingLiquidType, "rising liquid earthquake");
        AssertEqual(0x0020, RoomEarthquakeDefinitions.RisingLiquidTimerBits, "rising liquid timer bits");
        AssertEqual(6, RoomEarthquakeDefinitions.SoundSuppressedRooms.Count, "silent quake rooms");
        AssertEqual(RoomHeaderDefinitions.BombTorizo, RoomEarthquakeDefinitions.SoundSuppressedRooms[0], "first silent quake room");

        // RoomFxRomData's layer-three values, and the import's pages and blends.
        AssertEqual(0x184e, Layer3Plane.ClearedCell.PackedWord, "cleared effect cell");
        AssertEqual(3, RoomFxPaletteBlendDefinitions.ColorCount, "palette blend colours");
        AssertEqual(0x8a8000 + 5 * 0x840, RoomFxLayer3TilemapSources.Page(RoomFxType.Fog).Value, "fog page source");
        AssertEqual(RoomFxLayer3TilemapSources.Page(RoomFxType.Water), RoomFxLayer3TilemapSources.Page(RoomFxType.TourianEntranceStatue),
            "the statue effect shows the water page");
        AssertEqual(0x89aa02 + 0x62, RoomFxLayer3TilemapSources.PaletteBlend(RoomFxPaletteBlend.Fog).Value, "fog blend source");
        // The FX records hold decoded fields; the record codec encodes them back to the bytes the catalog held before.
        RoomFxRecordDefinition climb = RoomFxDefinitions.ClimbState2.Records[0];
        AssertEqual(RoomFxType.Acid, climb.Type, "Climb FX type");
        AssertEqual(LayerBlendingConfiguration.LavaAcidAdditive, climb.Layer3LayerBlend, "Climb layer-three blend");
        AssertEqual(RoomLiquidOptions.SurfaceDrift, climb.LiquidOptions, "Climb liquid options");
        AssertEqual(RoomFxEffectSlots.Slot3 | RoomFxEffectSlots.Slot4 | RoomFxEffectSlots.Slot5, climb.PaletteFxSlots, "Climb palette-effect slots");
        AssertEqual((byte)0x04, RoomFxRecordEncoding.TypeByte(climb.Type), "Climb type byte");
        AssertEqual((byte)0x1e, RoomFxRecordEncoding.LayerBlendByte(climb.Layer3LayerBlend), "Climb blend byte");
        AssertEqual((byte)0x38, RoomFxRecordEncoding.SlotsByte(climb.PaletteFxSlots), "Climb palette-effect byte");
        AssertEqual(0x2c, SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.Types.Word(RoomFxType.CeresHaze), "Ceres haze type byte");
        AssertEqual(0x34, RoomFxRecordCodec.LayerBlends.Word(LayerBlendingConfiguration.MotherBrainPhaseTwo), "Mother Brain blend byte");
        AssertEqual(0xee, SuperMetroid.AssetExtraction.CartridgeTables.RoomFxRecordWords.PaletteBlends.Word(RoomFxPaletteBlend.MaridiaWaterD), "Maridia water D blend byte");
        AssertEqual("blend-EE", RoomFxPaletteBlendRules.Key(RoomFxPaletteBlend.MaridiaWaterD), "installed blend key keeps its name");
        AssertEqual((byte)0x80, RoomFxRecordEncoding.SlotsByte(RoomFxEffectSlotSet.Slot(7)), "eighth effect slot");
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
