namespace SuperMetroid.Core.Assets;

/// <summary>Editable JSON schema for the opening flashback's fourth-hit explosion compositions.</summary>
public sealed record IntroMotherBrainExplosionSpriteDocument
{
    /// <summary>Must match <see cref="IntroMotherBrainExplosionSpriteFormat.Version"/> before any frame is accepted.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the twelve named <c>small-explosion-0..5</c> and <c>big-explosion-0..5</c> compositions.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
