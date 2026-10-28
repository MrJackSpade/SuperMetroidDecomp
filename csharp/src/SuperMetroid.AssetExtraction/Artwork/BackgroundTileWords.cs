using SuperMetroid.Core.Assets;
using System.Buffers.Binary;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Converts a <see cref="BackgroundTile"/> to and from the packed sixteen-bit tilemap entry the cartridge stores it as:
/// the import pipeline decodes cartridge tilemaps and block tables into tiles once, and encodes tiles back where an
/// imported file or a content identity keeps the cartridge's words. It belongs to the import pipeline; it stays in Core
/// only while the room effects' pages, the boss BG2 bodies, the message boxes and the frontend screens still hold words
/// (#1422).
/// </summary>
public static class BackgroundTileWords
{
    /// <summary>The tile a packed tilemap entry places.</summary>
    public static BackgroundTile ToTile(int word)
    {
        var entry = new SnesBgTilemapWord(checked((ushort)word));
        return new BackgroundTile(entry.CharacterIndex, entry.PaletteIndex, entry.FlipHorizontally, entry.FlipVertically, entry.HasPriority);
    }

    /// <summary>The packed tilemap entry that places <paramref name="tile"/>.</summary>
    public static ushort ToWord(BackgroundTile tile) =>
        SnesBgTilemapWord.Create(tile.Tile, tile.ColourVariant, tile.HighPriority, tile.Flip).Raw;

    /// <summary>The tiles packed tilemap entries place, in order.</summary>
    public static BackgroundTile[] ToTiles(ReadOnlySpan<ushort> words)
    {
        var tiles = new BackgroundTile[words.Length];
        for (int index = 0; index < tiles.Length; index++)
            tiles[index] = ToTile(words[index]);
        return tiles;
    }

    /// <summary>The tiles little-endian packed tilemap entries place, in order.</summary>
    /// <exception cref="InvalidDataException">The bytes are not whole entries.</exception>
    public static BackgroundTile[] ToTiles(ReadOnlySpan<byte> littleEndianWords)
    {
        if (littleEndianWords.Length % sizeof(ushort) != 0)
            throw new InvalidDataException($"{littleEndianWords.Length} bytes are not whole tilemap entries.");
        var tiles = new BackgroundTile[littleEndianWords.Length / sizeof(ushort)];
        for (int index = 0; index < tiles.Length; index++)
            tiles[index] = ToTile(BinaryPrimitives.ReadUInt16LittleEndian(littleEndianWords[(index * sizeof(ushort))..]));
        return tiles;
    }

    /// <summary>Each tile's packed tilemap entry, in order.</summary>
    public static ushort[] ToWords(ReadOnlySpan<BackgroundTile> tiles)
    {
        var words = new ushort[tiles.Length];
        for (int index = 0; index < tiles.Length; index++)
            words[index] = ToWord(tiles[index]);
        return words;
    }

    /// <summary>Each tile's packed tilemap entry, as little-endian bytes in order.</summary>
    public static byte[] ToBytes(ReadOnlySpan<BackgroundTile> tiles)
    {
        var bytes = new byte[tiles.Length * sizeof(ushort)];
        for (int index = 0; index < tiles.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * sizeof(ushort)), ToWord(tiles[index]));
        return bytes;
    }
}
