using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The ending's object artwork as the import pipeline reads it (#633, #1463): the atlases and tilemaps the game binds,
/// with the sprite compositions its sheets are composed from.
/// </summary>
public sealed record EndingObjectArtworkParts : EndingObjectSceneParts
{
    /// <summary>Selected atmospheric-cloud OAM compositions.</summary>
    public required EndingCloudSpritePresentation CloudSprites { get; init; }

    /// <summary>Selected Zebes-explosion OAM compositions.</summary>
    public required EndingExplosionSpritePresentation ExplosionSprites { get; init; }

    /// <summary>Selected completion-message and clear-time OAM compositions.</summary>
    public required EndingCompletionTextSpritePresentation CompletionTextSprites { get; init; }

    /// <summary>Selected post-credits Samus reward OAM compositions.</summary>
    public required EndingRewardSpritePresentation RewardSprites { get; init; }

    /// <summary>Selected assembling-logo OAM compositions.</summary>
    public required EndingLogoSpritePresentation LogoSprites { get; init; }
}
