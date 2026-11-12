namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>One period of what a library background shows (#1651, #1675): its tilemap words, in rows of its width.</summary>
/// <param name="Words">The period's tilemap words, row by row.</param>
/// <param name="WidthTiles">Tiles across one row of the period: a page, or less where the page repeats across.</param>
internal sealed record RoomBackgroundPeriod(ushort[] Words, int WidthTiles);
