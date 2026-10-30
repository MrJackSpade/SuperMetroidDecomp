using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Layout-only content: tile-sheet coordinates, palette selectors, priority and flips; the import's alone (#1467).</summary>
public sealed record MapScreenDocument
{
    /// <summary>Gets the map-screen schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets all named 32-by-32 map-screen tile grids.</summary>
    public required Dictionary<string, MapPresentationCell[]> Pages { get; init; }
}
