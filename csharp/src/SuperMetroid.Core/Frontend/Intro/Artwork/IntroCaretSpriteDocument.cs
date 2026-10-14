using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Editable JSON schema for ordered visible-caret sprite parts; positioning and blink timing remain cinematic-script behavior, not authored frame sequences.</summary>
public sealed record IntroCaretSpriteDocument
{
    /// <summary>Schema revision: current <see cref="IntroCaretSpriteVersion.Current"/> or the supported <see cref="IntroCaretSpriteVersion.Previous"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Current schema requires <c>caret-visible</c> with at most 128 ordered OAM parts; legacy schema requires <c>caret-still</c> and three <c>caret-blink-1..3</c> names, but only the still composition is installed.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
