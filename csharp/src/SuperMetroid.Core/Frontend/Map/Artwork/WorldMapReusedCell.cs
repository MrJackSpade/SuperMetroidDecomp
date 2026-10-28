namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>A foreground cell drawn as another cell's pixels.</summary>
/// <param name="Source">The cell whose pixels are shown.</param>
/// <param name="Mirroring">The mirrorings the source is shown with; both together turn it half a revolution.</param>
internal readonly record struct WorldMapReusedCell(int Source, TileFlip Mirroring);
