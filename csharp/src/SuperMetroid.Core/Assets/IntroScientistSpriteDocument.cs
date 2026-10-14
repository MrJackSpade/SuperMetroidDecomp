namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON schema for the scientist scenes' baby-Metroid compositions.</summary>
public sealed record IntroScientistSpriteDocument
{
    /// <summary>Must match <see cref="IntroScientistSpriteFormat.Version"/> before any visual frame is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>Gets all ten required named baby-Metroid visual compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
