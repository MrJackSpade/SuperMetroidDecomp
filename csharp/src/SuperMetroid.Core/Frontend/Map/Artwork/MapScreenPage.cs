namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>One page of the map screen's artwork: its id and tile layout.</summary>
/// <param name="Id">The page's asset id.</param>
/// <param name="TileColumns">Tiles per row.</param>
/// <param name="TileCount">How many tiles the page holds.</param>
public readonly record struct MapScreenPage(string Id, int TileColumns, int TileCount);
