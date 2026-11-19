using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable completion-message OAM schema, independent of the fixed bank-$8B typewriter pacing, program transitions, and recorded clear-time values.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the ending completion text's packed sprite parts, which only the import reads to compose its pictures; EndingObjectArtworkFiles.Extract writes it to the ending objects' import data, which the installer deletes before it publishes.")]
public sealed record EndingCompletionTextSpriteDocument
{
    /// <summary>Composition schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 56 named ordered part arrays: operation-00..14, completed-00..20, clear-time-00..08, digit-00..09, and colon-00; null part palettes inherit the drawing owner's palette.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
