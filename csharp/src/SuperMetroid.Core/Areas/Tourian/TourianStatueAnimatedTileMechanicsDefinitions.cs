using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Immutable engine-owned headers and control programs for the four Tourian entrance
/// statue animated-tile objects in bank <c>$87</c>.
/// </summary>
/// <remarks>
/// The nine frame-source operands in each program are presentation references in
/// <see cref="SuperMetroid.Core.Areas.Tourian.Artwork.TourianStatueAnimatedTileArtworkDefinitions"/>.
/// This catalog owns the instruction graph, timing, event routing, boss-bit
/// tests, palette destinations, and effect parameters that make the sequence operate.
/// </remarks>
public static class TourianStatueAnimatedTileMechanicsDefinitions
{
    /// <summary>First timed source operand, $87:83B8 relative to Phantoon's $83AC entry.</summary>
    internal const int FirstOscillatingSource = 0x0c;
    /// <summary>First post-palette-clear source operand, $87:83E6 relative to $83AC.</summary>
    internal const int FirstReleaseSource = 0x3a;
    /// <summary>Source operand for the eye-glow wait, $87:83F2 relative to $83AC.</summary>
    internal const int EyeGlowSource = 0x46;
    /// <summary>Source operand for the soul/palette-FX wait, $87:83FE relative to $83AC.</summary>
    internal const int SoulSource = 0x52;
    /// <summary>
    /// Native boss identity selects its animation program, transfer geometry, event,
    /// boss test and palette/effect destinations. These are semantic object cases,
    /// not sampled numeric curves. The six-byte headers are $87:854C..8563 and their
    /// 104-byte programs are $87:83AC..854B; unsupported ushort identities return null.
    /// </summary>
    private static TourianStatueAnimatedTileProgramDefinition? SelectObject(AnimatedTileObject objectPointer) =>
        objectPointer switch
    {
        AnimatedTileObject.TourianStatuePhantoon => new(new TourianStatueAnimatedTileProgramParts { ObjectPointer = AnimatedTileObject.TourianStatuePhantoon, ProgramStart = 0x83ac, TransferByteCount = 0x0080, EncodedVramDestination = 0x7800, StatueStateBit = 0x0001, GreyEventNumber = 0x0006, FirstFrameDuration = 0x0006, PackedBossTest = 0x0301, ClearPaletteByteIndex = 0x0158, UnlockEffectParameter = 0x0000, PaletteFxDefinition = 0xf755, TargetPaletteByteIndex = 0x0140 }),
        AnimatedTileObject.TourianStatueRidley => new(new TourianStatueAnimatedTileProgramParts { ObjectPointer = AnimatedTileObject.TourianStatueRidley, ProgramStart = 0x8414, TransferByteCount = 0x0040, EncodedVramDestination = 0x7220, StatueStateBit = 0x0002, GreyEventNumber = 0x0007, FirstFrameDuration = 0x000a, PackedBossTest = 0x0201, ClearPaletteByteIndex = 0x0132, UnlockEffectParameter = 0x0002, PaletteFxDefinition = 0xf751, TargetPaletteByteIndex = 0x0120 }),
        AnimatedTileObject.TourianStatueKraid => new(new TourianStatueAnimatedTileProgramParts { ObjectPointer = AnimatedTileObject.TourianStatueKraid, ProgramStart = 0x847c, TransferByteCount = 0x0040, EncodedVramDestination = 0x0b40, StatueStateBit = 0x0004, GreyEventNumber = 0x0009, FirstFrameDuration = 0x0004, PackedBossTest = 0x0101, ClearPaletteByteIndex = 0x00f8, UnlockEffectParameter = 0x0006, PaletteFxDefinition = 0xf74d, TargetPaletteByteIndex = 0x00e0 }),
        AnimatedTileObject.TourianStatueDraygon => new(new TourianStatueAnimatedTileProgramParts { ObjectPointer = AnimatedTileObject.TourianStatueDraygon, ProgramStart = 0x84e4, TransferByteCount = 0x0080, EncodedVramDestination = 0x0ca0, StatueStateBit = 0x0008, GreyEventNumber = 0x0008, FirstFrameDuration = 0x0008, PackedBossTest = 0x0401, ClearPaletteByteIndex = 0x00d2, UnlockEffectParameter = 0x0004, PaletteFxDefinition = 0xf749, TargetPaletteByteIndex = 0x00c0 }),
        AnimatedTileObject.None or AnimatedTileObject.Empty or AnimatedTileObject.HorizontalSpikes or
            AnimatedTileObject.VerticalSpikes or AnimatedTileObject.CrateriaLake or
            AnimatedTileObject.UnusedCrateriaLava or AnimatedTileObject.BrinstarPlant or
            AnimatedTileObject.WreckedShipScreen or AnimatedTileObject.MaridiaSandCeiling or
            AnimatedTileObject.MaridiaSandFalling or AnimatedTileObject.WreckedShipTreadmillRightwards or
            AnimatedTileObject.WreckedShipTreadmillLeftwards or AnimatedTileObject.Lava or
            AnimatedTileObject.Acid or AnimatedTileObject.Rain or AnimatedTileObject.Spores => null,
        _ => throw new InvalidOperationException($"Undefined AnimatedTileObject {objectPointer}."),
    };

    /// <summary>The definition of an object this catalog supports.</summary>
    private static TourianStatueAnimatedTileProgramDefinition Require(AnimatedTileObject objectPointer) => SelectObject(objectPointer) ??
        throw new InvalidOperationException($"{objectPointer} has no definition in this catalog.");

    /// <summary>The four boss-statue programs in cartridge header order, without a stored roster.</summary>
    public static IEnumerable<TourianStatueAnimatedTileProgramDefinition> All
    {
        get
        {
            yield return Require(AnimatedTileObject.TourianStatuePhantoon);
            yield return Require(AnimatedTileObject.TourianStatueRidley);
            yield return Require(AnimatedTileObject.TourianStatueKraid);
            yield return Require(AnimatedTileObject.TourianStatueDraygon);
        }
    }

    /// <summary>Resolves one stock bank-$87 statue animated-tile object header.</summary>
    public static bool TryResolveObjectHeader(
        AnimatedTileObject objectPointer,
        [NotNullWhen(true)] out TourianStatueAnimatedTileProgramDefinition? definition)
    {
        definition = SelectObject(objectPointer);
        return definition is not null;
    }
}
