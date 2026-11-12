using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The ending's installed object documents (#633, #1463, #1467, #1636): its maps and sprite compositions, which its pictures
/// and sheets are composed from and named by. Its characters are import data the installation never holds
/// (<see cref="EndingObjectCharacters"/>).
/// </summary>
public sealed record EndingObjectArtworkParts
{

    /// <summary>Four 32-by-32-cell explosion maps, ordered by <see cref="EndingObjectFragmentId"/>.</summary>
    public required IReadOnlyList<RoomBackgroundTilemapAtlas> Fragments { get; init; }

    /// <summary>$0800-byte, 32-by-32-cell BG2 waiting-scene map.</summary>
    public required RoomBackgroundTilemapAtlas WaitingTilemap { get; init; }

    /// <summary>32-by-32-cell BG3 transformation map.</summary>
    public required RoomBackgroundTilemapAtlas PostCreditsFragmentB { get; init; }

    /// <summary>$0800-byte, 32-by-32-cell final-logo map.</summary>
    public required RoomBackgroundTilemapAtlas PostShotLogoMap { get; init; }

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
