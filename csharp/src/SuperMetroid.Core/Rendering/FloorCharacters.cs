using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What a frontend screen's Mode 7 characters show (#1392): each installed artwork it loads, as the pre-coloured sheet of
/// its characters, at the Mode 7 tiles it loads them to; a later load shows over an earlier one.
/// </summary>
public sealed class FloorCharacters
{
    /// <summary>The tile each Mode 7 tile shows, null where nothing pictured has loaded.</summary>
    private readonly BgTilePicture?[] tiles = new BgTilePicture?[FloorTilePictures.Tiles];

    /// <summary>Whether each tile holds characters nothing pictures; a tile never loaded holds zeros and shows nothing.</summary>
    private readonly bool[] unpictured = new bool[FloorTilePictures.Tiles];

    /// <summary>The pictures last read, until a load changes them.</summary>
    private FloorTilePictures? pictures;

    /// <summary>Records <paramref name="count"/> characters of <paramref name="sheet"/>'s artwork from <paramref name="firstCharacter"/>, loaded to Mode 7 tiles from <paramref name="firstTile"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet holds fewer characters, or the load passes the last tile.</exception>
    public FloorCharacters Show(Mode7CharacterSheet sheet, int firstTile, int firstCharacter, int count)
    {
        Ensure.NotNull(sheet);
        Ensure.BetweenInclusive(0, FloorTilePictures.Tiles - 1, firstTile);
        Ensure.BetweenInclusive(1, FloorTilePictures.Tiles - firstTile, count);
        Ensure.BetweenInclusive(0, sheet.Tiles - count, firstCharacter);
        for (int tile = 0; tile < count; tile++)
            tiles[firstTile + tile] = new BgTilePicture(sheet.Picture, Mode7CharacterSheet.Corner(firstCharacter + tile));
        Array.Clear(unpictured, firstTile, count);
        pictures = null;
        return this;
    }

    /// <summary>Records a load of characters nothing pictures over <paramref name="count"/> Mode 7 tiles from <paramref name="firstTile"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The load passes the last tile.</exception>
    public FloorCharacters Clear(int firstTile, int count)
    {
        Ensure.BetweenInclusive(0, FloorTilePictures.Tiles - 1, firstTile);
        Ensure.BetweenInclusive(1, FloorTilePictures.Tiles - firstTile, count);
        Array.Clear(tiles, firstTile, count);
        Array.Fill(unpictured, true, firstTile, count);
        pictures = null;
        return this;
    }

    /// <summary>What each Mode 7 tile shows.</summary>
    public FloorTilePictures Pictures() => pictures ??= new FloorTilePictures([.. tiles], [.. unpictured]);
}
