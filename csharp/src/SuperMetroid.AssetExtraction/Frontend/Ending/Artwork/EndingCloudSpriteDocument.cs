using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>JSON schema for the editable atmospheric ending-cloud compositions.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the ending clouds' packed sprite parts, which only the import reads to compose their pictures; EndingObjectArtworkFiles.Extract writes it to the ending objects' import data, which the installer deletes before it publishes.")]
public sealed record EndingCloudSpriteDocument
{
    /// <summary>Gets the schema version, which must equal <see cref="EndingCloudSpriteFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the compositions keyed by the published cloud-frame names.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
