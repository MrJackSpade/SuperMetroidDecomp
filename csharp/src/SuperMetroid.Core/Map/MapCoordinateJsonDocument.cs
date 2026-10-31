using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Map;

/// <summary>A zero-based cell in an area's 64-by-32 map grid.</summary>
/// <param name="X">Horizontal map-tile coordinate, from 0 through 63.</param>
/// <param name="Y">Vertical map-tile coordinate, from 0 through 31.</param>
[SpatialComponents("External format: save JSON stores each explored map cell as x/y.")]
public sealed record MapCoordinateJsonDocument(int X, int Y)
{
    /// <summary>Encodes a map cell for the save document.</summary>
    public static MapCoordinateJsonDocument FromCell(Position16<MapTiles> cell) => new(cell.X, cell.Y);

    /// <summary>The persisted map cell.</summary>
    [JsonIgnore]
    public Position16<MapTiles> Cell => new(X, Y);
}
