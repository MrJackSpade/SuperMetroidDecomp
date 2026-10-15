using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>JSON schema for the editable atmospheric ending-cloud compositions.</summary>
public sealed record EndingCloudSpriteDocument
{
    /// <summary>Gets the schema version, which must equal <see cref="EndingCloudSpriteFormat.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the compositions keyed by the published cloud-frame names.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
