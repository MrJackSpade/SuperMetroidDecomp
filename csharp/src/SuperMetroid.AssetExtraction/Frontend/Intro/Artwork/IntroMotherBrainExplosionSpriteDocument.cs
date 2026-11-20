using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Editable JSON schema for the opening flashback's fourth-hit explosion compositions.</summary>
public sealed record IntroMotherBrainExplosionSpriteDocument
{
    /// <summary>Must match <see cref="IntroMotherBrainExplosionSpriteFormat.Version"/> before any frame is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the twelve compositions, each named for the explosion pose it shows (<c>small-explosion-spark</c>, <c>big-explosion-scattered-smoke</c>).</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
