namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>One cell of a Mode 7 floor's map (#1403): the tile it shows.</summary>
/// <param name="Tile">The tile shown, 0 through one less than <see cref="FloorTilePictures.Tiles"/>.</param>
public readonly record struct AffineFloorCell(int Tile)
{
    /// <summary>The tile shown, 0 through one less than <see cref="FloorTilePictures.Tiles"/>.</summary>
    public int Tile
    {
        get;
        init => field = Ensure.BetweenInclusive(0, FloorTilePictures.Tiles - 1, value);
    } = Ensure.BetweenInclusive(0, FloorTilePictures.Tiles - 1, Tile);
}
