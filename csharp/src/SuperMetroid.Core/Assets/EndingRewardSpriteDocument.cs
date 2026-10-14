namespace SuperMetroid.Core.Assets;

/// <summary>Defines the complete named post-credits Samus sprite set.</summary>
public sealed record EndingRewardSpriteDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the exact 37-frame dictionary of ordered sprite parts.</summary>
    public required Dictionary<string, SpriteVisualPart[]> Frames { get; init; }
}
