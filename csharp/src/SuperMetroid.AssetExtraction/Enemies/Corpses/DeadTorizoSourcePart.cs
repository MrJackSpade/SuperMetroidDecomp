using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>One square part of the dead Torizo's source sheet: its top-left tile cell and its size in tiles.</summary>
/// <param name="Corner">The part's top-left tile cell in the source sheet.</param>
/// <param name="Extent">The part's width and height, in tiles.</param>
internal readonly record struct DeadTorizoSourcePart(Position16<TilemapCells> Corner, int Extent);
