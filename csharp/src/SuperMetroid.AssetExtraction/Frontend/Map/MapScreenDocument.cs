using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Layout-only content: tile-sheet coordinates, palette selectors, priority and flips; the import's alone (#1467).</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the map screens' packed tile placements, which only the import reads to picture the screens; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportDocumentFiles), which the installer deletes before it publishes.")]
public sealed record MapScreenDocument
{
    /// <summary>Gets the map-screen schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets all named 32-by-32 map-screen tile grids.</summary>
    public required Dictionary<string, MapPresentationCell[]> Pages { get; init; }
}
