using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The codec between a room-FX record's sixteen cartridge bytes and its typed <see cref="RoomEffectDefinition"/>
/// (#1415): the room-FX catalog generator decodes the retail records once, and checks that seed synthetic records encode
/// through it. Core holds only the decoded fields: a record's two effect bitsets select from its room's area's lists
/// (<see cref="AreaPaletteFxObjects"/>, <see cref="AreaBlockAnimations"/>), and the decoded record names the effects
/// they select (#1526). The record's two blend bytes select entries of the game's blending dispatcher; the import reads
/// them once into the room effect's own blend and whether the room animates Samus's visor (#1545), and its options byte
/// once into the liquid the effect fills the room with (#1695); the numbers stay here. Never linked by player hosts.
/// </summary>
internal static class RoomFxRecordCodec
{
    /// <summary>The blend byte that configures nothing, as a record of no effect holds it.</summary>
    internal const byte UnconfiguredBlendByte = 0x00;
    /// <summary>The blend byte of ordinary gameplay: no effect overlay blends with the room.</summary>
    internal const byte NormalBlendByte = 0x02;
    /// <summary>The default blend byte of the Wrecked Ship's rooms before its power returns; the port draws nothing of it.</summary>
    internal const byte WreckedShipPowerOffBlendByte = 0x08;
    /// <summary>The effect blend byte of the spores, which lie behind the room and brighten it.</summary>
    internal const byte SporesBlendByte = 0x0a;
    /// <summary>The effect blend byte of the rain, whose overlay brightens the room.</summary>
    internal const byte RainBlendByte = 0x0e;
    /// <summary>The effect blend byte of water whose overlay darkens the room.</summary>
    internal const byte WaterDarkensBlendByte = 0x14;
    /// <summary>The effect blend byte of the waterfall rooms' water, which lies behind the room with the background and darkens it.</summary>
    internal const byte WaterfallBlendByte = 0x16;
    /// <summary>The effect blend byte of water or the Tourian statues' water whose overlay brightens the room.</summary>
    internal const byte LiquidBrightensBlendByte = 0x18;
    /// <summary>The effect blend byte of lava and acid, whose overlay brightens the room.</summary>
    internal const byte LavaAcidBlendByte = 0x1e;
    /// <summary>The first of the two default blend bytes whose rooms animate Samus's visor.</summary>
    internal const byte VisorBackdropBlendByte = 0x28;
    /// <summary>The second of the two default blend bytes whose rooms animate Samus's visor.</summary>
    internal const byte VisorBackdropAlternateBlendByte = 0x2a;
    /// <summary>The effect blend byte of the fog, whose overlay brightens the room.</summary>
    internal const byte FogBlendByte = 0x30;

    /// <summary>
    /// Whether a room whose record holds default blend byte <paramref name="value"/> animates Samus's visor (#1545): the
    /// two backdrop entries do; nothing, ordinary gameplay and the Wrecked Ship's unpowered rooms do not.
    /// </summary>
    /// <exception cref="InvalidDataException">The byte names no default blend a retail room holds; the message names <paramref name="context"/>.</exception>
    internal static RoomVisorAnimation VisorAnimation(byte value, string context) => value switch
    {
        VisorBackdropBlendByte or VisorBackdropAlternateBlendByte => RoomVisorAnimation.Animated,
        UnconfiguredBlendByte or NormalBlendByte or WreckedShipPowerOffBlendByte => RoomVisorAnimation.Steady,
        _ => throw new InvalidDataException($"{context} default blend ${value:X2} names no room's blend."),
    };

    /// <summary>
    /// The blend a room effect of <paramref name="type"/> shows when its record holds effect blend byte
    /// <paramref name="value"/> (#1545), or null for an effect that shows no overlay. Each effect that shows a page names
    /// its own blend, water any of its three; every other effect names none.
    /// </summary>
    /// <exception cref="InvalidDataException">The byte names no blend, or one the effect does not show; the message names <paramref name="context"/>.</exception>
    internal static RoomEffectBlend? EffectBlend(RoomFxType type, byte value, string context)
    {
        RoomEffectBlend? blend = value switch
        {
            UnconfiguredBlendByte or NormalBlendByte => null,
            LavaAcidBlendByte or LiquidBrightensBlendByte or FogBlendByte or RainBlendByte => RoomEffectBlendDefinitions.OverlayBrightens,
            WaterDarkensBlendByte => RoomEffectBlendDefinitions.OverlayDarkens,
            WaterfallBlendByte => RoomEffectBlendDefinitions.WaterfallDarkensBehind,
            SporesBlendByte => RoomEffectBlendDefinitions.SporesBrightenBehind,
            _ => throw new InvalidDataException($"{context} effect blend ${value:X2} names no room effect's blend."),
        };
        if (!EffectShowsBlend(type, value))
            throw new InvalidDataException($"{context} effect blend ${value:X2} is not a blend a {type} effect shows.");
        return blend;
    }

    /// <summary>Whether an effect of <paramref name="type"/> shows the blend effect blend byte <paramref name="value"/> names.</summary>
    private static bool EffectShowsBlend(RoomFxType type, byte value) => type switch
    {
        RoomFxType.Lava or RoomFxType.Acid => value == LavaAcidBlendByte,
        RoomFxType.Water or RoomFxType.TourianEntranceStatue =>
            value is WaterDarkensBlendByte or WaterfallBlendByte or LiquidBrightensBlendByte,
        RoomFxType.Rain => value == RainBlendByte,
        RoomFxType.Fog => value == FogBlendByte,
        RoomFxType.Spores => value == SporesBlendByte,
        RoomFxType.None or RoomFxType.ScrollingSky or RoomFxType.Fireflea or
            RoomFxType.CeresRidley or RoomFxType.CeresElevator or RoomFxType.CeresHaze =>
            value is UnconfiguredBlendByte or NormalBlendByte,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown room effect."),
    };

    /// <summary>
    /// Decodes the sixteen bytes of a record in a room of <paramref name="area"/>, whose lists its effect bits select from.
    /// The record's door word is its entry door in its room's effect selection (<see cref="RoomFxSelection"/>), which the catalog generator reads.
    /// </summary>
    /// <exception cref="InvalidDataException">A field names no effect, blend or palette blend, or the effect blend is not one the effect shows; the message names <paramref name="context"/>.</exception>
    internal static RoomEffectDefinition Decode(AreaId area, ReadOnlySpan<byte> native, string context)
    {
        Ensure.LengthEqual(native, RoomFxToolingRomData.Record.ByteCount);
        RoomFxType type = RoomFxRecordWords.Types.Decode(native[RoomFxRomDataRecordTooling.TypeOffset], $"{context} room-FX type");
        return new(
            SurfaceY(Word(native, RoomFxRomDataRecordTooling.BaseYPositionOffset)),
            SurfaceY(Word(native, RoomFxRomDataRecordTooling.TargetYPositionOffset)),
            new SignedEightEight(Word(native, RoomFxRomDataRecordTooling.YVelocityOffset)),
            native[RoomFxRomDataRecordTooling.TimerOffset],
            type,
            VisorAnimation(native[RoomFxRomDataRecordTooling.DefaultBlendOffset], context),
            EffectBlend(type, native[RoomFxRomDataRecordTooling.EffectBlendOffset], context),
            Liquid(type, native[RoomFxRomDataRecordTooling.LiquidOptionsOffset]),
            PaletteEffects(area, native[RoomFxRomDataRecordTooling.PaletteFxBitsetOffset]),
            AnimatedTiles(area, native[RoomFxRomDataRecordTooling.AnimatedTileBitsetOffset]),
            RoomFxRecordWords.PaletteBlends.Decode(native[RoomFxRomDataRecordTooling.PaletteBlendOffset], $"{context} palette blend"));

        static ushort Word(ReadOnlySpan<byte> bytes, int offset) => LittleEndianBytes.Word(bytes[offset], bytes[offset + 1]);
    }

    /// <summary>The height word a record holds where it names no surface height.</summary>
    internal const ushort UnnamedSurfaceWord = ushort.MaxValue;

    /// <summary>The surface height a record's height word names, or null for <see cref="UnnamedSurfaceWord"/>.</summary>
    internal static int? SurfaceY(ushort word) => word == UnnamedSurfaceWord ? null : word;

    /// <summary>The options-byte bit that makes water's surface drift; lava and acid do not read it.</summary>
    private const int SurfaceDriftBit = 1 << 0;

    /// <summary>The options-byte bit that gives water its background wave, and lava or acid its vertical heat haze.</summary>
    private const int BackgroundWaveBit = 1 << 1;

    /// <summary>
    /// The options-byte bit that makes Samus ignore water's surface until a room object enables it, and gives lava or acid
    /// its horizontal heat haze unless <see cref="BackgroundWaveBit"/> selects the vertical one.
    /// </summary>
    private const int SurfaceIgnoredOrHorizontalHazeBit = 1 << 2;

    /// <summary>An options-byte bit some records set that no liquid reads; the import drops it.</summary>
    private const int UnreadBit = 1 << 3;

    /// <summary>The options-byte bit that selects the large tide, unless <see cref="SmallTideBit"/> is also set.</summary>
    private const int LargeTideBit = 1 << 6;

    /// <summary>The options-byte bit that selects the small tide, which wins over <see cref="LargeTideBit"/>.</summary>
    private const int SmallTideBit = 1 << 7;

    /// <summary>Every options-byte bit the import knows; a record setting any other names nothing.</summary>
    private const int KnownBits = SurfaceDriftBit | BackgroundWaveBit | SurfaceIgnoredOrHorizontalHazeBit | UnreadBit | LargeTideBit | SmallTideBit;

    /// <summary>
    /// The liquid a record of <paramref name="type"/> whose options byte is <paramref name="value"/> fills its room with
    /// (#1695): water for water and the statue effect, lava or acid for those, and none for every other effect, which reads
    /// none of the byte. Each liquid keeps only the facts it has: lava and acid drop the drift bit, every liquid the unread
    /// bit, and the small tide wins when both tide bits are set.
    /// </summary>
    /// <exception cref="InvalidDataException">The byte sets a bit that selects no option.</exception>
    internal static RoomLiquid? Liquid(RoomFxType type, byte value)
    {
        int unnamed = value & ~KnownBits;
        if (unnamed != 0)
            throw new InvalidDataException($"Liquid options ${value:X2} set bits ${unnamed:X2}, which select no option.");
        bool Has(int bit) => (value & bit) != 0;
        LiquidTide tide = Has(SmallTideBit) ? LiquidTide.Small : Has(LargeTideBit) ? LiquidTide.Large : LiquidTide.None;
        return type switch
        {
            RoomFxType.Water or RoomFxType.TourianEntranceStatue =>
                new RoomWater(tide, Has(SurfaceDriftBit), Has(BackgroundWaveBit), Has(SurfaceIgnoredOrHorizontalHazeBit)),
            RoomFxType.Lava or RoomFxType.Acid => new RoomLavaOrAcid(tide,
                Has(BackgroundWaveBit) ? HeatHazeDirection.Vertical
                : Has(SurfaceIgnoredOrHorizontalHazeBit) ? HeatHazeDirection.Horizontal
                : HeatHazeDirection.None),
            RoomFxType.None or RoomFxType.Rain or RoomFxType.Fog or RoomFxType.Spores or RoomFxType.ScrollingSky or
                RoomFxType.Fireflea or RoomFxType.CeresRidley or RoomFxType.CeresElevator or
                RoomFxType.CeresHaze => null,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown room effect."),
        };
    }

    /// <summary>
    /// The options byte that selects <paramref name="liquid"/>: the byte checks seed synthetic records with, and the word
    /// the native capture seed writes for the room's water.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="liquid"/> is no known liquid, or holds a tide or haze that names none.</exception>
    internal static byte LiquidOptionsByte(RoomLiquid? liquid) => checked((byte)(liquid switch
    {
        null => 0,
        RoomWater water => TideBits(water.Tide) |
            (water.SurfaceDrift ? SurfaceDriftBit : 0) |
            (water.BackgroundWave ? BackgroundWaveBit : 0) |
            (water.IgnoresSurfaceUntilEnabled ? SurfaceIgnoredOrHorizontalHazeBit : 0),
        RoomLavaOrAcid lavaOrAcid => TideBits(lavaOrAcid.Tide) | lavaOrAcid.Haze switch
        {
            HeatHazeDirection.None => 0,
            HeatHazeDirection.Horizontal => SurfaceIgnoredOrHorizontalHazeBit,
            HeatHazeDirection.Vertical => BackgroundWaveBit,
            _ => throw new ArgumentOutOfRangeException(nameof(liquid), lavaOrAcid.Haze, "Unknown heat haze."),
        },
        _ => throw new ArgumentOutOfRangeException(nameof(liquid), liquid, "Unknown room liquid."),
    }));

    /// <summary>The options-byte bits that select <paramref name="tide"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tide"/> names no tide.</exception>
    private static int TideBits(LiquidTide tide) => tide switch
    {
        LiquidTide.None => 0,
        LiquidTide.Small => SmallTideBit,
        LiquidTide.Large => LargeTideBit,
        _ => throw new ArgumentOutOfRangeException(nameof(tide), tide, "Unknown liquid tide."),
    };

    /// <summary>
    /// The palette effects a record's palette-effect byte starts in a room of <paramref name="area"/>, in ascending bit
    /// order, leaving out <see cref="PaletteFxObject.Nothing"/>, whose program only stops.
    /// </summary>
    internal static ImmutableArray<PaletteFxObject> PaletteEffects(AreaId area, byte value) =>
        [.. SetBits(value).Select(bit => AreaPaletteFxObjects.Read(area, bit) ?? throw new InvalidDataException(
                $"A {area} record sets palette-effect bit {bit}, which selects the glow copy no retail room sets."))
            .Where(effect => effect != PaletteFxObject.Nothing)];

    /// <summary>
    /// The block animations a record's block-animation byte starts in a room of <paramref name="area"/>, in ascending
    /// bit order. No retail record sets a bit that selects no object of the game (#1686), and the decode rejects one.
    /// </summary>
    internal static ImmutableArray<BlockAnimation> AnimatedTiles(AreaId area, byte value) =>
        [.. SetBits(value).Select(bit => AreaBlockAnimations.Read(area, bit) ?? throw new InvalidDataException(
            $"A {area} record sets block-animation bit {bit}, which selects no block animation of the game."))];

    /// <summary>The bits <paramref name="value"/> sets, lowest first.</summary>
    private static IEnumerable<int> SetBits(byte value) =>
        Enumerable.Range(0, AreaBlockAnimationDefinitions.ObjectsPerArea).Where(bit => (value & (1 << bit)) != 0);
}
