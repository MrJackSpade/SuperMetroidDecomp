using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx;

/// <summary>Encodes typed room-FX record fields back to their cartridge bytes, for checks that seed synthetic records
/// and compare the import's decode with the cartridge (#1415).</summary>
internal static class RoomFxRecordEncoding
{
    /// <summary>
    /// The height word that holds <paramref name="y"/>: <see cref="RoomFxRecordCodec.UnnamedSurfaceWord"/> where the record
    /// names none.
    /// </summary>
    internal static ushort SurfaceWord(int? y) => y is int named ? checked((ushort)named) : RoomFxRecordCodec.UnnamedSurfaceWord;

    /// <summary>The record byte naming <paramref name="type"/>.</summary>
    internal static byte TypeByte(RoomFxType type) => (byte)RoomFxRecordWords.Types.Word(type);

    /// <summary>The record byte naming <paramref name="blend"/>.</summary>
    internal static byte PaletteBlendByte(RoomFxPaletteBlend blend) => (byte)RoomFxRecordWords.PaletteBlends.Word(blend);

    /// <summary>
    /// The palette-effect byte that starts <paramref name="effects"/> in a room of <paramref name="area"/>: each effect's
    /// bit in the area's list (#1526).
    /// </summary>
    /// <exception cref="InvalidDataException">An effect is not in the area's list once, or the effects are out of the list's order.</exception>
    internal static byte PaletteEffectsByte(ImmutableArray<PaletteFxObject> effects, AreaId area) =>
        BitsetByte(effects, bit => AreaPaletteFxObjects.Read(area, bit), $"{area} palette effect");

    /// <summary>
    /// The animated-tile byte that starts <paramref name="tiles"/> in a room of <paramref name="area"/>: each object's bit
    /// in the area's list (#1526).
    /// </summary>
    /// <exception cref="InvalidDataException">An object is not in the area's list once, or the objects are out of the list's order.</exception>
    internal static byte AnimatedTilesByte(ImmutableArray<AnimatedTileObject> tiles, AreaId area) =>
        BitsetByte(tiles, bit => AreaAnimatedTileObjects.Read(area, bit), $"{area} animated-tile object");

    /// <summary>
    /// The bitset whose bits select <paramref name="effects"/> from the list <paramref name="selected"/> reads, the effects
    /// in ascending bit order as the decode lists them.
    /// </summary>
    /// <exception cref="InvalidDataException">An effect is not in the list once, or the effects are out of the list's order.</exception>
    private static byte BitsetByte<TEffect>(ImmutableArray<TEffect> effects, Func<int, TEffect?> selected, string kind)
        where TEffect : struct, Enum
    {
        int bitset = 0;
        int previous = -1;
        foreach (TEffect effect in effects)
        {
            int[] bits = [.. Enumerable.Range(0, AreaAnimatedTileObjectDefinitions.ObjectsPerArea)
                .Where(bit => selected(bit) is { } candidate && EqualityComparer<TEffect>.Default.Equals(candidate, effect))];
            if (bits.Length != 1)
                throw new InvalidDataException($"{kind} {effect} is selected by {bits.Length} bits of its area's list, not one.");
            if (bits[0] <= previous)
                throw new InvalidDataException($"{kind} {effect} is out of its area's list order.");
            previous = bits[0];
            bitset |= 1 << bits[0];
        }
        return checked((byte)bitset);
    }
}
