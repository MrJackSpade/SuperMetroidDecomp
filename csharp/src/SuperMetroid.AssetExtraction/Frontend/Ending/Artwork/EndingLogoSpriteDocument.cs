using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>JSON schema for the editable final-logo sprite compositions.</summary>
public sealed record EndingLogoSpriteDocument
{
    /// <summary>Gets the schema version, which must equal <see cref="EndingLogoSpriteFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the compositions keyed by the published frame names.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
