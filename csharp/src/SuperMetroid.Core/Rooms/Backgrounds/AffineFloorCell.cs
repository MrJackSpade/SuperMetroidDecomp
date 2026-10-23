namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>One cell of a Mode 7 floor's map (#1403): the tile it shows.</summary>
/// <param name="Tile">The tile shown, 0 through one less than <see cref="Mode7TilePictures.Tiles"/>.</param>
public readonly record struct AffineFloorCell(int Tile)
{
    /// <summary>The tile shown, 0 through one less than <see cref="Mode7TilePictures.Tiles"/>.</summary>
    public int Tile
    {
        get;
        init => field = Ensure.BetweenInclusive(0, Mode7TilePictures.Tiles - 1, value);
    } = Ensure.BetweenInclusive(0, Mode7TilePictures.Tiles - 1, Tile);
}
