using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The point and segment artwork of a grapple tile set.</summary>
/// <param name="Point">The point.</param>
/// <param name="Segments">The segments.</param>
public readonly partial record struct GrappleTileSetKey(InstalledArtworkId Point, InstalledArtworkId Segments);
