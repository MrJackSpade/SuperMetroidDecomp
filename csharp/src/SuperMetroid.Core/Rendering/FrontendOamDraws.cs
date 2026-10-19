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
    private const int NameSelectBit = 1 << 8;

    /// <summary>The values of OAM's nine-bit X: the upper half lies left of the screen.</summary>
    private const int XRange = 512;

    /// <summary>The values of OAM's eight-bit Y.</summary>
    private const int YRange = 256;

    /// <summary>Characters in one OBJ name table.</summary>
    private const int NameTableCharacters = 256;

    /// <summary>OBSEL bits 7-5: the small and large object sizes of each size mode.</summary>
    private static readonly (int SmallWidth, int SmallHeight, int LargeWidth, int LargeHeight)[] SizeModes =
    [
        (8, 8, 16, 16),
        (8, 8, 32, 32),
        (8, 8, 64, 64),
        (16, 16, 32, 32),
        (16, 16, 64, 64),
        (32, 32, 64, 64),
        (16, 32, 32, 64),
        (16, 32, 32, 32),
    ];

    /// <summary>Appends the draw <paramref name="oam"/>'s finalized records stand for, when it has any.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="oam">The screen's finalized OAM storage.</param>
    /// <param name="vram">The screen's VRAM, which holds the characters the records name.</param>
    /// <param name="objectSelection">The screen's OBSEL value.</param>
    public static void Append(SpriteFrameBuffer sprites, OamBuffer oam, SnesVram vram, int objectSelection)
    {
        Ensure.NotNull(sprites);
        if (Draw(oam, vram, objectSelection) is { } draw)
            sprites.Add(draw);
    }

    /// <summary>The draws <paramref name="oam"/>'s finalized records stand for: none, or one draw of all their cells.</summary>
    /// <param name="oam">The screen's finalized OAM storage.</param>
    /// <param name="vram">The screen's VRAM, which holds the characters the records name.</param>
    /// <param name="objectSelection">The screen's OBSEL value.</param>
    public static ImmutableArray<SpriteDraw> Draws(OamBuffer oam, SnesVram vram, int objectSelection) =>
        Draw(oam, vram, objectSelection) is { } draw ? [draw] : [];

    /// <summary>The one draw of every finalized record's cells, or null when there is no record.</summary>
    private static SpriteDraw? Draw(OamBuffer oam, SnesVram vram, int objectSelection)
    {
        Ensure.NotNull(oam);
        Ensure.NotNull(vram);
        byte obsel = checked((byte)objectSelection);
        if (oam.LastFinalizedSpriteCount == 0)
            return null;
        var characters = new byte[ObjCharacterPage.Characters * SpriteArtwork.CharacterByteCount];
        for (int name = 0; name < ObjCharacterPage.Characters; name++)
            vram.Bytes.Slice(ResolveTileByteAddress(name, obsel), SpriteArtwork.CharacterByteCount)
                .CopyTo(characters.AsSpan(name * SpriteArtwork.CharacterByteCount));
        var page = new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.FrontendObjects), characters);
        (int smallWidth, int smallHeight, int largeWidth, int largeHeight) = SizeModes[(obsel >> 5) & 7];
        var parts = ImmutableArray.CreateBuilder<SpriteFramePart>();
        for (int index = 0; index < oam.LastFinalizedSpriteCount; index++)
        {
            OamEntry entry = oam.GetEntry(index);
            int width = entry.IsLarge ? largeWidth : smallWidth;
            int height = entry.IsLarge ? largeHeight : smallHeight;
            // OAM X is a signed/modular 9-bit coordinate and Y wraps above the visible field.
            int objectX = entry.Position.X >= XRange / 2 ? entry.Position.X - XRange : entry.Position.X;
            int objectY = entry.Position.Y >= SnesPpuLayout.ScreenHeightPixels ? entry.Position.Y - YRange : entry.Position.Y;
            var flip = (entry.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (entry.FlipY ? SnesTileFlipFlags.Vertical : 0);
            int columns = width / SpriteArtwork.CharacterPixels;
            int rows = height / SpriteArtwork.CharacterPixels;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    // Names occupy a sixteen-wide grid; the ninth name-select bit stays fixed when the low byte wraps.
                    int name = (entry.TileNumber & NameSelectBit) | unchecked((byte)(entry.TileNumber + row * 16 + column));
                    int shownColumn = entry.FlipX ? columns - 1 - column : column;
                    int shownRow = entry.FlipY ? rows - 1 - row : row;
                    parts.Add(new SpriteFramePart(
                        new Offset16<ScreenPixels>(checked((short)(objectX + shownColumn * SpriteArtwork.CharacterPixels)),
                            checked((short)(objectY + shownRow * SpriteArtwork.CharacterPixels))),
                        new Extent16<ScreenPixels>(SpriteArtwork.CharacterPixels, SpriteArtwork.CharacterPixels),
                        name, flip, entry.Palette, entry.Priority));
                }
            }
        }
        var frame = new SpriteFrame($"frontend OAM, {oam.LastFinalizedSpriteCount} records", parts.ToImmutable());
        return new SpriteDraw(new SpriteSource(SpriteSourceKind.FrontendObjects), frame, page, default, 0, 0, 0);
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
