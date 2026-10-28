namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend tile layer's tile set as its screen lays it out (#1404): which pre-coloured tile each of the layer's tile
/// numbers shows, placed run by run from installed artwork sheets. A later placement shows over an earlier one; a tile
/// number nothing is placed at shows nothing.
/// </summary>
public sealed class ScreenTileSet
{
    /// <summary>The tile each tile number shows, or none.</summary>
    private readonly ScreenTilePicture?[] tiles = new ScreenTilePicture?[ScreenLayerPictures.CharacterIndices];

    /// <summary>The pictures of the placements so far, until another placement changes them.</summary>
    private ScreenLayerPictures? pictures;

    /// <summary>Starts an empty set of <paramref name="depth"/> tiles.</summary>
    public ScreenTileSet(TileInkDepth depth) => Depth = depth;

    /// <summary>The depth of the artwork the set's tiles come from; every placement is of artwork this deep.</summary>
    public TileInkDepth Depth { get; }

    /// <summary>
    /// Places <paramref name="count"/> characters of <paramref name="sheet"/>'s artwork, from <paramref name="firstCharacter"/>,
    /// at consecutive tile numbers from <paramref name="firstTile"/>. Characters placed past the last tile number are not shown.
    /// </summary>
    /// <exception cref="ArgumentException">The sheet's artwork is not as deep as the set's.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The sheet holds fewer characters, or the first tile number is not one of the set's.</exception>
    public ScreenTileSet Place(ScreenTileSheet sheet, int firstTile, int firstCharacter, int count)
    {
        Ensure.NotNull(sheet);
        Ensure.BetweenInclusive(0, sheet.TileCount - 1, firstCharacter);
        Ensure.BetweenInclusive(1, sheet.TileCount - firstCharacter, count);
        return Fill(sheet, firstTile, firstCharacter, count, 1);
    }

    /// <summary>Places <paramref name="copies"/> copies of <paramref name="sheet"/>'s <paramref name="character"/> at consecutive tile numbers from <paramref name="firstTile"/>.</summary>
    /// <exception cref="ArgumentException">The sheet's artwork is not as deep as the set's.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The sheet holds no such character, or the first tile number is not one of the set's.</exception>
    public ScreenTileSet PlaceRepeated(ScreenTileSheet sheet, int firstTile, int character, int copies)
    {
        Ensure.NotNull(sheet);
        Ensure.BetweenInclusive(0, sheet.TileCount - 1, character);
        Ensure.BetweenInclusive(1, int.MaxValue, copies);
        return Fill(sheet, firstTile, character, copies, 0);
    }

    /// <summary>The tile each tile number shows.</summary>
    public ScreenLayerPictures Pictures() => pictures ??= new ScreenLayerPictures([.. tiles], Depth);

    /// <summary>Places <paramref name="count"/> tiles, each <paramref name="step"/> characters on from the one before.</summary>
    private ScreenTileSet Fill(ScreenTileSheet sheet, int firstTile, int firstCharacter, int count, int step)
    {
        if (sheet.Depth != Depth)
            throw new ArgumentException($"A {Depth} tile set cannot show {sheet.Id}'s {sheet.Depth} artwork.", nameof(sheet));
        Ensure.BetweenInclusive(0, ScreenLayerPictures.CharacterIndices - 1, firstTile);
        int shown = Math.Min(count, ScreenLayerPictures.CharacterIndices - firstTile);
        for (int index = 0; index < shown; index++)
            tiles[firstTile + index] = new ScreenTilePicture(sheet, firstCharacter + index * step);
        pictures = null;
        return this;
    }
}
