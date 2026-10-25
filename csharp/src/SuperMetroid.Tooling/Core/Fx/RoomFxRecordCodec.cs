using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The codec between a room-FX record's sixteen cartridge bytes and its typed <see cref="RoomFxRecordDefinition"/>
/// (#1415): the room-FX catalog generator decodes the retail records once, and checks that seed synthetic records encode
/// through it. Core holds only the decoded fields. Never linked by player hosts.
/// </summary>
[CpuBusBoundary("codec: room-FX record bytes ↔ RoomFxRecordDefinition")]
internal static class RoomFxRecordCodec
{
    /// <summary>The bank-$88 dispatcher byte naming each <see cref="LayerBlendingConfiguration"/>.</summary>
    internal static readonly NativeWordIdentities<LayerBlendingConfiguration> LayerBlends = new(new Dictionary<LayerBlendingConfiguration, ushort>
    {
        [LayerBlendingConfiguration.Unconfigured] = 0x00,
        [LayerBlendingConfiguration.NormalGameplay] = 0x02,
        [LayerBlendingConfiguration.PhantoonHidden] = 0x04,
        [LayerBlendingConfiguration.UnusedSemiTransparentSprites] = 0x06,
        [LayerBlendingConfiguration.WreckedShipPowerOff] = 0x08,
        [LayerBlendingConfiguration.Spores] = 0x0a,
        [LayerBlendingConfiguration.Fireflea] = 0x0c,
        [LayerBlendingConfiguration.Rain] = 0x0e,
        [LayerBlendingConfiguration.MorphBallEye] = 0x10,
        [LayerBlendingConfiguration.SuitPickup] = 0x12,
        [LayerBlendingConfiguration.WaterSubtractive] = 0x14,
        [LayerBlendingConfiguration.WaterfallSubtractive] = 0x16,
        [LayerBlendingConfiguration.LiquidOrFogAdditive] = 0x18,
        [LayerBlendingConfiguration.PhantoonSemiTransparent] = 0x1a,
        [LayerBlendingConfiguration.UnusedHalfAdditiveReversedBackgrounds] = 0x1c,
        [LayerBlendingConfiguration.LavaAcidAdditive] = 0x1e,
        [LayerBlendingConfiguration.NormalGameplayAlternate] = 0x20,
        [LayerBlendingConfiguration.UnusedWaterSubtractive] = 0x22,
        [LayerBlendingConfiguration.MotherBrainWindow] = 0x24,
        [LayerBlendingConfiguration.UnusedHalfAdditive] = 0x26,
        [LayerBlendingConfiguration.VisorBackdrop28] = 0x28,
        [LayerBlendingConfiguration.VisorBackdrop2A] = 0x2a,
        [LayerBlendingConfiguration.HazeOrTorizo] = 0x2c,
        [LayerBlendingConfiguration.UnusedSubtractive] = 0x2e,
        [LayerBlendingConfiguration.FogAdditive] = 0x30,
        [LayerBlendingConfiguration.UnusedSubtractiveBackground] = 0x32,
        [LayerBlendingConfiguration.MotherBrainPhaseTwo] = 0x34,
    });

    /// <summary>The record that ends a list: every field cleared.</summary>
    internal static RoomFxRecordDefinition Terminator { get; } = new(null, true, 0, 0, 0, 0, RoomFxType.None,
        LayerBlendingConfiguration.Unconfigured, LayerBlendingConfiguration.Unconfigured, RoomLiquidOptions.None,
        RoomFxEffectSlots.None, RoomFxEffectSlots.None, RoomFxPaletteBlend.None);

    /// <summary>Decodes the sixteen bytes of a record that applies to <paramref name="door"/>, or is its list's default when null.</summary>
    /// <exception cref="InvalidDataException">A field names no effect, blend or palette blend; the message names <paramref name="context"/>.</exception>
    internal static RoomFxRecordDefinition Decode(DoorDefinition? door, ReadOnlySpan<byte> native, string context)
    {
        Ensure.LengthEqual(native, RoomFxToolingRomData.Record.ByteCount);
        return new(door, false,
            Word(native, RoomFxRomDataRecordTooling.BaseYPositionOffset),
            Word(native, RoomFxRomDataRecordTooling.TargetYPositionOffset),
            Word(native, RoomFxRomDataRecordTooling.YVelocityOffset),
            native[RoomFxRomDataRecordTooling.TimerOffset],
            RoomFxRecordWords.Types.Decode(native[RoomFxRomDataRecordTooling.TypeOffset], $"{context} room-FX type"),
            LayerBlends.Decode(native[RoomFxRomDataRecordTooling.DefaultLayerBlendConfigurationOffset], $"{context} default layer blend"),
            LayerBlends.Decode(native[RoomFxRomDataRecordTooling.Layer3LayerBlendConfigurationOffset], $"{context} layer-three blend"),
            LiquidOptions(native[RoomFxRomDataRecordTooling.LiquidOptionsOffset]),
            Slots(native[RoomFxRomDataRecordTooling.PaletteFxBitsetOffset]),
            Slots(native[RoomFxRomDataRecordTooling.AnimatedTileBitsetOffset]),
            RoomFxRecordWords.PaletteBlends.Decode(native[RoomFxRomDataRecordTooling.PaletteBlendOffset], $"{context} palette blend"));

        static ushort Word(ReadOnlySpan<byte> bytes, int offset) => SnesLittleEndian.Word(bytes[offset], bytes[offset + 1]);
    }

    /// <summary>The liquid options a record's options byte selects; every bit is kept, tested or not.</summary>
    internal static RoomLiquidOptions LiquidOptions(byte value) => (RoomLiquidOptions)value;

    /// <summary>The effect slots a record's slot byte turns on.</summary>
    internal static RoomFxEffectSlots Slots(byte value) => (RoomFxEffectSlots)value;
}
