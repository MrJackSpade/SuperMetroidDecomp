namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Adds a picture layer's colours onto the screen (#1467), as the world map adds an area's background to its foreground; an
/// optional covering layer limits the addition to where that layer shows an opaque pixel. Sprites stay where the screen
/// places them, and the addition never reaches them.
/// </summary>
/// <param name="Pictures">The added layer's placed pictures, unscrolled.</param>
/// <param name="Coverage">The layer the screen shows, whose opaque pixels the addition is limited to; everywhere when null.</param>
public sealed record AddedPicturesLayer(PlacedPictures Pictures, PicturesLayer? Coverage) : DisplayLayer
{
    /// <summary>The added layer's placed pictures.</summary>
    public PlacedPictures Pictures
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Pictures);
}
