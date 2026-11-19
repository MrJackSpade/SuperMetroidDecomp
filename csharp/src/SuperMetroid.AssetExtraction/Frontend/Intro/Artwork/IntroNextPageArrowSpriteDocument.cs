using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// Editable JSON schema for the Japanese text's next-page arrow compositions (#1655); its position, cadence and hidden delay
/// remain the narration's behaviour, not authored frames.
/// </summary>
public sealed record IntroNextPageArrowSpriteDocument
{
    /// <summary>Must match <see cref="IntroNextPageArrowSpriteFormat.Version"/> before any visual frame is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>The arrow's three named visual compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
