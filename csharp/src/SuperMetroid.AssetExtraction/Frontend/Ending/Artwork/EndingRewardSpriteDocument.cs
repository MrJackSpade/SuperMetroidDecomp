using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Defines the complete named post-credits Samus sprite set.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the ending reward's packed sprite parts, which only the import reads to compose its pictures; EndingObjectArtworkFiles.Extract writes it to the ending objects' import data, which the installer deletes before it publishes.")]
public sealed record EndingRewardSpriteDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the exact 37-frame dictionary of ordered sprite parts.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
