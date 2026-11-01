namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Drawing positions only: moving an icon never changes its compiled discovery cell.</summary>
public sealed class MapStationPositions
{
    /// <summary>Owned screen-position lookup by stable discovery marker ID, separate from compiled discovery cells.</summary>
    private readonly Dictionary<string, Position16<MapPixels>> points;
    /// <summary>Stores a mapping already checked for the complete marker set and bounded screen coordinates.</summary>
    internal MapStationPositions(Dictionary<string, Position16<MapPixels>> points) => this.points = points;

    /// <summary>Gets the screen-pixel drawing position for a named map-station marker.</summary>
    /// <param name="id">The stable marker identifier from the station discovery rules.</param>
    /// <returns>The marker's configured screen position.</returns>
    /// <exception cref="InvalidDataException">The marker identifier is unknown.</exception>
    public Position16<MapPixels> Get(string id) => points.TryGetValue(id, out var point) ? point
        : throw new InvalidDataException($"Unknown map station marker '{id}'.");

}
