using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Converts a frontend screen's OAM storage into the whole-frame draw it stands for, so both backends draw sprites only
/// from draws. Frontend screens still write OAM records until #1358 moves them onto sprite entries; this bridge then
/// goes. Each record becomes its 8x8 character cells, read through OBSEL from the screen's VRAM with the native name
/// wrap, so addressing, flips, palettes and priorities match the record, and the first record stays frontmost.
/// </summary>
[PpuLayoutBoundary("bridge: frontend OAM records and their OBSEL-addressed VRAM characters, until #1358 moves the frontend writers")]
public static class FrontendOamDraws
{
    /// <summary>The ninth OBJ character-name bit, selecting the second name table.</summary>
    internal const int NameSelectBit = 1 << 8;

    /// <summary>Characters in one OBJ name table.</summary>
    private const int NameTableCharacters = 256;

    /// <summary>The OBJ characters <paramref name="vram"/> holds under <paramref name="objectSelection"/>, as one page in name order.</summary>
    public static SpriteArtwork Page(SnesVram vram, int objectSelection)
    {
        Ensure.NotNull(vram);
        byte obsel = checked((byte)objectSelection);
        var characters = new byte[ObjCharacterPage.Characters * SpriteArtwork.CharacterByteCount];
        for (int name = 0; name < ObjCharacterPage.Characters; name++)
            vram.Bytes.Slice(ResolveTileByteAddress(name, obsel), SpriteArtwork.CharacterByteCount)
                .CopyTo(characters.AsSpan(name * SpriteArtwork.CharacterByteCount));
        return new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.FrontendObjects), characters);
    }

    /// <summary>Resolves a nine-bit OBJ character name through OBSEL into byte VRAM.</summary>
    /// <param name="tileNumber">The name, 0 through 511.</param>
    /// <param name="obsel">The OBSEL value.</param>
    public static int ResolveTileByteAddress(int tileNumber, byte obsel)
    {
        if ((uint)tileNumber >= 2 * NameTableCharacters)
            throw new ArgumentOutOfRangeException(nameof(tileNumber));
        // OBSEL bits 0-2 choose the base in $2000-word ($4000-byte) units. A set ninth
        // tile bit selects the second name table, whose programmable displacement is
        // ((bits 3-4) + 1) * $1000 words. Each 4-bpp 8x8 tile occupies 16 VRAM words.
        int baseWord = (obsel & 7) << 13;
        int nameSelectWordOffset = (((obsel >> 3) & 3) + 1) << 12;
        int tileWord = baseWord + unchecked((byte)tileNumber) * 16;
        if ((tileNumber & NameSelectBit) != 0)
            tileWord += nameSelectWordOffset;
        // Physical VRAM has only 15 word-address bits, so register arithmetic wraps.
        return (tileWord & (SnesPpuLayout.VramWordCount - 1)) * 2;
    }
}
