using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The loads the cartridge's ending has made into its screen memory by a phase (#1515): which escape or explosion floor,
/// whether the flyaway's floor, the post-credits art, the reward icon and the explosion's maps are in place, and how many
/// of the logo's transfers the post-shot has made. The game shows the pictures its phase shows and records none of this;
/// checks that compare the installed artwork with the cartridge's memory image derive it from the phase.
/// </summary>
/// <param name="FloorScene">The escape or explosion scene whose floor is in place, or none.</param>
/// <param name="FlyawayFloor">Whether the flyaway's floor has been transferred over the explosion's.</param>
/// <param name="PostCreditsArt">Whether the post-credits art is in place.</param>
/// <param name="RewardIcon">Whether the reward icon's floor is in place.</param>
/// <param name="ExplosionMaps">Whether the explosion's objects, maps and the ending font are in place.</param>
/// <param name="PostShotParts">How many of the logo's transfers the post-shot has made.</param>
internal sealed record EndingScreenLoads(EndingFloorSceneId? FloorScene, bool FlyawayFloor, bool PostCreditsArt, bool RewardIcon,
    bool ExplosionMaps, int PostShotParts)
{
    /// <summary>The transfers the post-shot makes of the logo, the last of which completes it.</summary>
    internal const int PostShotPartCount = 6;

    /// <summary>No load made: the screen memory as a full clear leaves it.</summary>
    internal static EndingScreenLoads None { get; } = new(null, false, false, false, false, 0);

    /// <summary>
    /// The loads the cartridge has made by <paramref name="phase"/>: each escape scene's floor from its setup, the
    /// explosion's floor and maps from its setup until the credits clear them, the flyaway's floor from the explosion's
    /// animation, the post-credits art from the credits, the reward icon once the shot turns into the camera and the logo
    /// once the shot has handed over to the white flash.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The phase is undefined.</exception>
    internal static EndingScreenLoads Of(EndingCreditsPhase phase) => phase switch
    {
        EndingCreditsPhase.SetupEscapeFromZebes => None,
        EndingCreditsPhase.WaitForEscapeMusic or EndingCreditsPhase.FadeInSideCloudEscape or EndingCreditsPhase.SideCloudEscape
            or EndingCreditsPhase.FadeOutSideCloudEscape => None with { FloorScene = EndingFloorSceneId.SideCloudEscape },
        EndingCreditsPhase.FadeInTopAndBottomCloudEscape or EndingCreditsPhase.TopAndBottomCloudEscape
            or EndingCreditsPhase.FadeOutTopAndBottomCloudEscape => None with { FloorScene = EndingFloorSceneId.TopAndBottomCloudEscape },
        EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionPaletteCrossfade
            or EndingCreditsPhase.ZebesExplosionHold => None with { FloorScene = EndingFloorSceneId.PlanetExplosion, ExplosionMaps = true },
        EndingCreditsPhase.ZebesExplosionAnimation or EndingCreditsPhase.WaitForPlanetEscapeMusic
            or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue or EndingCreditsPhase.PlanetEscapeFast
            or EndingCreditsPhase.PlanetEscapeSlow or EndingCreditsPhase.PlanetEscapeAccelerating
            or EndingCreditsPhase.OperationSuccessfulText or EndingCreditsPhase.FadeOutToCredits =>
            None with { FloorScene = EndingFloorSceneId.PlanetExplosion, FlyawayFloor = true, ExplosionMaps = true },
        EndingCreditsPhase.Credits or EndingCreditsPhase.PostCreditsBlank or EndingCreditsPhase.PostCreditsFadeIn
            or EndingCreditsPhase.PostCreditsShootingStars or EndingCreditsPhase.PostCreditsWaitingBackdrop
            or EndingCreditsPhase.PostCreditsWaitingSamus or EndingCreditsPhase.PostCreditsReward
            or EndingCreditsPhase.PostCreditsCopyright or EndingCreditsPhase.PostCreditsGesture
            or EndingCreditsPhase.PostCreditsJump => None with { PostCreditsArt = true },
        EndingCreditsPhase.PostCreditsShot => None with { PostCreditsArt = true, RewardIcon = true },
        EndingCreditsPhase.PostCreditsWhiteFlash or EndingCreditsPhase.PostCreditsLogo or EndingCreditsPhase.ItemPercentage
            or EndingCreditsPhase.ItemPercentageScrollDown or EndingCreditsPhase.SeeYouNextMission =>
            None with { PostCreditsArt = true, RewardIcon = true, PostShotParts = PostShotPartCount },
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined ending phase."),
    };
}
