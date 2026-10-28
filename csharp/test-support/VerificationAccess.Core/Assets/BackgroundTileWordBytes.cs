namespace SuperMetroid.Core.Assets;

/// <summary>
/// Encodes placed background tiles as the cartridge's packed tilemap entries through the import pipeline's codec, so
/// verification can compare the port's tiles with the words the cartridge holds.
/// </summary>
internal static class BackgroundTileWordBytes
{
    /// <summary>Each tile's packed tilemap entry, in order.</summary>
    internal static ushort[] Words(ReadOnlySpan<BackgroundTile> tiles) => BackgroundTileWords.ToWords(tiles);

    /// <summary>Each tile's packed tilemap entry, as little-endian bytes in order.</summary>
    internal static byte[] Of(ReadOnlySpan<BackgroundTile> tiles) => BackgroundTileWords.ToBytes(tiles);

    /// <summary>The tiles little-endian packed tilemap entries place, in order.</summary>
    internal static BackgroundTile[] Tiles(ReadOnlySpan<byte> littleEndianWords) => BackgroundTileWords.ToTiles(littleEndianWords);

    /// <summary>The tiles packed tilemap entries place, in order.</summary>
    internal static BackgroundTile[] Tiles(ReadOnlySpan<ushort> words) => BackgroundTileWords.ToTiles(words);

    /// <summary>The tile one packed tilemap entry places.</summary>
    internal static BackgroundTile Tile(int word) => BackgroundTileWords.ToTile(checked((ushort)word));

    /// <summary>The cartridge's packed tilemap entry for a placed tile.</summary>
    extension(BackgroundTile tile)
    {
        /// <summary>The packed tilemap entry the cartridge stores this placement as.</summary>
        internal ushort PackedWord => BackgroundTileWords.ToWord(tile);
    }
}
