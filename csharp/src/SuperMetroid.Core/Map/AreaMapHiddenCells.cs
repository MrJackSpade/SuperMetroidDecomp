namespace SuperMetroid.Core.Map;

/// <summary>
/// The cells the map screens show in place of a cell nothing reveals (#1466): the empty cell, and the different empty
/// cell the room-select map shows for an area whose map Samus has not downloaded. The import pipeline names them.
/// </summary>
/// <param name="Unrevealed">The cell the pause and room-select maps show where nothing is revealed.</param>
/// <param name="WithoutAreaMap">The cell the room-select map shows where nothing is revealed in an area without its map.</param>
public sealed record AreaMapHiddenCells(MapCell Unrevealed, MapCell WithoutAreaMap)
{
    /// <summary>The cell the room-select map shows where nothing is revealed, whether or not the area's map is downloaded.</summary>
    /// <param name="hasAreaMap">Whether Samus has downloaded the area's map.</param>
    public MapCell RoomSelect(bool hasAreaMap) => hasAreaMap ? Unrevealed : WithoutAreaMap;
}
