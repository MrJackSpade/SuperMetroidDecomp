namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The point and segment artwork of a grapple tile set.</summary>
/// <param name="Point">The point.</param>
/// <param name="Segments">The segments.</param>
public readonly record struct GrappleTileSetKey(VramAssetId Point, VramAssetId Segments);
