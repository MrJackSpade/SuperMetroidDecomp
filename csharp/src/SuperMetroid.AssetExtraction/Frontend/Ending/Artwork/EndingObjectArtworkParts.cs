using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The ending's object artwork as the import pipeline reads it (#633, #1463): the tilemaps the game binds, with the
/// character atlases and sprite compositions its sheets are composed from.
/// </summary>
public sealed record EndingObjectArtworkParts : EndingObjectSceneParts
{
    /// <summary>$4000-byte atmospheric-cloud OBJ character sheet.</summary>
    public required RoomCharacterAtlas Clouds { get; init; }

    /// <summary>$6000-byte Zebes-explosion OBJ character sheet.</summary>
    public required RoomCharacterAtlas Explosion { get; init; }

    /// <summary>$4000-byte waiting-Samus character sheet, also used for suited rewards.</summary>
    public required RoomCharacterAtlas WaitingSamus { get; init; }

    /// <summary>$4000-byte Samus-shooting OBJ sheet.</summary>
    public required RoomCharacterAtlas ShootingScreen { get; init; }

    /// <summary>$4000-byte suitless reward OBJ sheet.</summary>
    public required RoomCharacterAtlas SuitlessSamus { get; init; }

    /// <summary>$0100-byte BG3 transformation-character transfer.</summary>
    public required RoomCharacterAtlas PostCreditsFragmentA { get; init; }

    /// <summary>$2000-byte logo character stream divided into four staged uploads.</summary>
    public required RoomCharacterAtlas PostShotLogoTiles { get; init; }

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
