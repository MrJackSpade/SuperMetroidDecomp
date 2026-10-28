using System.Buffers.Binary;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Encodes placed background tiles as the cartridge's packed tilemap entries, so verification can compare the port's
/// tiles with the words the cartridge holds.
/// </summary>
internal static class BackgroundTileWordBytes
{
    /// <summary>Each tile's packed tilemap entry, in order.</summary>
    internal static ushort[] Words(ReadOnlySpan<BackgroundTile> tiles)
    {
        var words = new ushort[tiles.Length];
        for (int index = 0; index < tiles.Length; index++)
            words[index] = BackgroundTileWords.ToWord(tiles[index]);
        return words;
    }

    /// <summary>Each tile's packed tilemap entry, as little-endian bytes in order.</summary>
    internal static byte[] Of(ReadOnlySpan<BackgroundTile> tiles)
    {
        var bytes = new byte[tiles.Length * sizeof(ushort)];
        for (int index = 0; index < tiles.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * sizeof(ushort)), BackgroundTileWords.ToWord(tiles[index]));
        return bytes;
    }

    /// <summary>The tiles little-endian packed tilemap entries place, in order.</summary>
    internal static BackgroundTile[] Tiles(ReadOnlySpan<byte> littleEndianWords)
    {
        var tiles = new BackgroundTile[littleEndianWords.Length / sizeof(ushort)];
        for (int index = 0; index < tiles.Length; index++)
            tiles[index] = BackgroundTileWords.ToTile(BinaryPrimitives.ReadUInt16LittleEndian(littleEndianWords[(index * sizeof(ushort))..]));
        return tiles;
    }

    /// <summary>The tiles packed tilemap entries place, in order.</summary>
    internal static BackgroundTile[] Tiles(ReadOnlySpan<ushort> words)
    {
        var tiles = new BackgroundTile[words.Length];
        for (int index = 0; index < tiles.Length; index++)
            tiles[index] = BackgroundTileWords.ToTile(words[index]);
        return tiles;
    }
}
