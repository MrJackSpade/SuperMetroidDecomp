using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Defines every named map-screen sprite composition.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the map sprites' packed sprite parts, which only the import reads to compose their pictures; MapPresentationExtractor.Extract writes it to the map presentation's import data (MapPresentationFiles.ImportDocumentFiles), which the installer deletes before it publishes.")]
public sealed record MapSpriteDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the exact 26-frame dictionary of ordered OBJ parts.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
