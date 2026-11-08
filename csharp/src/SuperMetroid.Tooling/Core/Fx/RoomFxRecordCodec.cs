using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The codec between a room-FX record's sixteen cartridge bytes and its typed <see cref="RoomFxRecordDefinition"/>
/// (#1415): the room-FX catalog generator decodes the retail records once, and checks that seed synthetic records encode
/// through it. Core holds only the decoded fields: a record's two effect bitsets select from its room's area's lists
/// (<see cref="AreaPaletteFxObjects"/>, <see cref="AreaAnimatedTileObjects"/>), and the decoded record names the effects
/// they select (#1526). Never linked by player hosts.
/// </summary>
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
    internal static RoomFxRecordDefinition Terminator { get; } = new(null, true, 0, 0, default, 0, RoomFxType.None,
        LayerBlendingConfiguration.Unconfigured, LayerBlendingConfiguration.Unconfigured, RoomLiquidOptions.None,
        [], [], RoomFxPaletteBlend.None);

    /// <summary>
    /// Decodes the sixteen bytes of a record that applies to <paramref name="door"/>, or is its list's default when null,
    /// in a room of <paramref name="area"/>, whose lists its effect bits select from.
    /// </summary>
    /// <exception cref="InvalidDataException">A field names no effect, blend or palette blend; the message names <paramref name="context"/>.</exception>
    internal static RoomFxRecordDefinition Decode(DoorDefinition? door, AreaId area, ReadOnlySpan<byte> native, string context)
    {
        Ensure.LengthEqual(native, RoomFxToolingRomData.Record.ByteCount);
        return new(door, false,
            SurfaceY(Word(native, RoomFxRomDataRecordTooling.BaseYPositionOffset)),
            SurfaceY(Word(native, RoomFxRomDataRecordTooling.TargetYPositionOffset)),
            new SignedEightEight(Word(native, RoomFxRomDataRecordTooling.YVelocityOffset)),
            native[RoomFxRomDataRecordTooling.TimerOffset],
            RoomFxRecordWords.Types.Decode(native[RoomFxRomDataRecordTooling.TypeOffset], $"{context} room-FX type"),
            LayerBlends.Decode(native[RoomFxRomDataRecordTooling.DefaultLayerBlendConfigurationOffset], $"{context} default layer blend"),
            LayerBlends.Decode(native[RoomFxRomDataRecordTooling.Layer3LayerBlendConfigurationOffset], $"{context} layer-three blend"),
            LiquidOptions(native[RoomFxRomDataRecordTooling.LiquidOptionsOffset]),
            PaletteEffects(area, native[RoomFxRomDataRecordTooling.PaletteFxBitsetOffset]),
            AnimatedTiles(area, native[RoomFxRomDataRecordTooling.AnimatedTileBitsetOffset]),
            RoomFxRecordWords.PaletteBlends.Decode(native[RoomFxRomDataRecordTooling.PaletteBlendOffset], $"{context} palette blend"))
        {
            AnimatesVisor = AnimatesVisor(native[RoomFxRomDataRecordTooling.DefaultLayerBlendConfigurationOffset]),
        };

        static ushort Word(ReadOnlySpan<byte> bytes, int offset) => LittleEndianBytes.Word(bytes[offset], bytes[offset + 1]);
    }

    /// <summary>
    /// Whether a record whose default layer-blend byte is <paramref name="defaultLayerBlend"/> animates Samus's visor: the
    /// room-FX loader stores that byte as the room's default blend ($89:AB40, cleared to 2 before each room at $88:8386),
    /// and the visor palette handler animates the visor only while it is $28 or $2A ($91:D849).
    /// </summary>
    internal static bool AnimatesVisor(byte defaultLayerBlend) => defaultLayerBlend is 0x28 or 0x2a;

    /// <summary>The height word a record holds where it names no surface height.</summary>
    internal const ushort UnnamedSurfaceWord = ushort.MaxValue;

    /// <summary>The surface height a record's height word names, or null for <see cref="UnnamedSurfaceWord"/>.</summary>
    internal static int? SurfaceY(ushort word) => word == UnnamedSurfaceWord ? null : word;

    /// <summary>The liquid options a record's options byte selects; every bit is kept, tested or not.</summary>
    internal static RoomLiquidOptions LiquidOptions(byte value) => (RoomLiquidOptions)value;

    /// <summary>
    /// The palette effects a record's palette-effect byte starts in a room of <paramref name="area"/>, in ascending bit
    /// order, leaving out <see cref="PaletteFxObject.Nothing"/>, whose program only stops.
    /// </summary>
    internal static ImmutableArray<PaletteFxObject> PaletteEffects(AreaId area, byte value) =>
        [.. SetBits(value).Select(bit => AreaPaletteFxObjects.Read(area, bit)).Where(effect => effect != PaletteFxObject.Nothing)];

    /// <summary>
    /// The animated-tile objects a record's animated-tile byte starts in a room of <paramref name="area"/>, in ascending
    /// bit order, leaving out <see cref="AnimatedTileObject.Empty"/>, the object an unused bit selects.
    /// </summary>
    internal static ImmutableArray<AnimatedTileObject> AnimatedTiles(AreaId area, byte value) =>
        [.. SetBits(value).Select(bit => AreaAnimatedTileObjects.Read(area, bit)).Where(tiles => tiles != AnimatedTileObject.Empty)];

    /// <summary>The bits <paramref name="value"/> sets, lowest first.</summary>
    private static IEnumerable<int> SetBits(byte value) =>
        Enumerable.Range(0, AreaAnimatedTileObjectDefinitions.ObjectsPerArea).Where(bit => (value & (1 << bit)) != 0);
}
