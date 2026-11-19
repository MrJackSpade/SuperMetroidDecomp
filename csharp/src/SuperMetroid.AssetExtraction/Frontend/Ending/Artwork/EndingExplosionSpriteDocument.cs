using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>JSON schema for the editable Zebes-explosion compositions.</summary>
[SuppressMessage("Design", "SME6463", Justification = "Import data: the ending explosions' packed sprite parts, which only the import reads to compose their pictures; EndingObjectArtworkFiles.Extract writes it to the ending objects' import data, which the installer deletes before it publishes.")]
public sealed record EndingExplosionSpriteDocument
{
    /// <summary>Schema revision checked before named explosion frames are compiled.</summary>
    public required int Version { get; init; }

    /// <summary>Editable compositions keyed by the 16 published planet, glow, starfield, silhouette, and afterglow names.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
