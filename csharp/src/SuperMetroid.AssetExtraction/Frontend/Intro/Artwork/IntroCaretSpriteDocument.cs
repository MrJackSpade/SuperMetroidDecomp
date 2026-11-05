using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Editable JSON schema for ordered visible-caret sprite parts; positioning and blink timing remain cinematic-script behavior, not authored frame sequences.</summary>
public sealed record IntroCaretSpriteDocument
{
    /// <summary>Schema revision; the reader accepts only <see cref="IntroCaretSpriteVersion.Current"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Requires <c>caret-visible</c> with at most 128 ordered OAM parts.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
