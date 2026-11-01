namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Boss, elevator and gunship drawing anchors, independent of progression and destination rules.</summary>
public sealed class MapLandmarkPositions
{
    /// <summary>Case-sensitive drawing anchors indexed by stable landmark identity.</summary>
    private readonly Dictionary<string, Position16<MapPixels>> points;
    /// <summary>Retains the complete, validated landmark coordinate set.</summary>
    internal MapLandmarkPositions(Dictionary<string, Position16<MapPixels>> points) => this.points = points;
    /// <summary>Gets a boss, elevator-label, or gunship drawing anchor in area-map pixels before the renderer subtracts map scrolling; does not determine whether the landmark is visible.</summary>
    /// <param name="id">Case-sensitive stable boss, elevator or gunship landmark identity.</param>
    /// <returns>The configured anchor with X in 0..511 and Y in 0..255.</returns>
    /// <exception cref="InvalidDataException">The landmark identity is not installed.</exception>
    public Position16<MapPixels> Get(string id) => points.TryGetValue(id, out var point) ? point
        : throw new InvalidDataException($"Unknown map landmark '{id}'.");

}
