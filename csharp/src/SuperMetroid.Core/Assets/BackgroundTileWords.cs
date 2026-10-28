namespace SuperMetroid.Core.Assets;

/// <summary>
/// Converts a <see cref="BackgroundTile"/> to and from the packed sixteen-bit tilemap entry the cartridge stores it as,
/// for the code #1422 has not yet moved off tilemap words.
/// </summary>
public static class BackgroundTileWords
{
    /// <summary>The tile a packed tilemap entry places.</summary>
    public static BackgroundTile ToTile(ushort word)
    {
        var entry = new SnesBgTilemapWord(word);
        return new BackgroundTile(entry.CharacterIndex, entry.PaletteIndex, entry.FlipHorizontally, entry.FlipVertically, entry.HasPriority);
    }

    /// <summary>The packed tilemap entry that places <paramref name="tile"/>.</summary>
    public static ushort ToWord(BackgroundTile tile) =>
        SnesBgTilemapWord.Create(tile.Tile, tile.ColourVariant, tile.HighPriority, tile.Flip).Raw;
}
