using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Editable JSON schema for the SR388 egg, remnants, and baby Metroid compositions.</summary>
public sealed record IntroDiscoveryActorSpriteDocument
{
    /// <summary>Must match <see cref="IntroDiscoveryActorSpriteFormat.Version"/> before any frame is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>Gets all twenty required named visual compositions in the compiled actor set.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
