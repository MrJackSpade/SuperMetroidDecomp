using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>JSON schema for the editable Zebes-explosion compositions.</summary>
public sealed record EndingExplosionSpriteDocument
{
    /// <summary>Schema revision checked before named explosion frames are compiled.</summary>
    public required int Version { get; init; }

    /// <summary>Editable compositions keyed by the 16 published planet, glow, starfield, silhouette, and afterglow names.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
