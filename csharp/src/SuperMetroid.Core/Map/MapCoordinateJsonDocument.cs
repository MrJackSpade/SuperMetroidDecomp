using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Map;

/// <summary>A zero-based cell in an area's map grid (<see cref="Progression.AreaMapSize"/>).</summary>
/// <param name="X">Horizontal map-tile coordinate, within the map's width.</param>
/// <param name="Y">Vertical map-tile coordinate, within the map's height.</param>
public sealed partial record MapCoordinateJsonDocument(int X, int Y)
{
    /// <summary>Encodes a map cell for the save document.</summary>
    public static MapCoordinateJsonDocument FromCell(Position16<MapTiles> cell) => new(cell.X, cell.Y);

    /// <summary>The persisted map cell.</summary>
    [JsonIgnore]
    public Position16<MapTiles> Cell => new(X, Y);
}
