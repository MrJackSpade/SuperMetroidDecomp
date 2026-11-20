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
    internal static EndingScreenLoads Of(EndingCreditsPhase phase)
    {
        foreach ((EndingCreditsPhase[] phases, EndingScreenLoads loads) in Arms)
        {
            if (phases.Contains(phase))
                return loads;
        }
        throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined ending phase.");
    }

    /// <summary>The loads each stretch of phases has made.</summary>
    private static (EndingCreditsPhase[] Phases, EndingScreenLoads Loads)[] Arms =>
    [
        ([EndingCreditsPhase.SetupEscapeFromZebes],
            None),
        ([EndingCreditsPhase.WaitForEscapeMusic, EndingCreditsPhase.FadeInSideCloudEscape, EndingCreditsPhase.SideCloudEscape, EndingCreditsPhase.FadeOutSideCloudEscape],
            None with { FloorScene = EndingFloorSceneId.SideCloudEscape }),
        ([EndingCreditsPhase.FadeInTopAndBottomCloudEscape, EndingCreditsPhase.TopAndBottomCloudEscape, EndingCreditsPhase.FadeOutTopAndBottomCloudEscape],
            None with { FloorScene = EndingFloorSceneId.TopAndBottomCloudEscape }),
        ([EndingCreditsPhase.FadeInZebesExplosion, EndingCreditsPhase.ZebesExplosionCrossfade, EndingCreditsPhase.ZebesExplosionHold],
            None with { FloorScene = EndingFloorSceneId.PlanetExplosion, ExplosionMaps = true }),
        ([EndingCreditsPhase.ZebesExplosionAnimation, EndingCreditsPhase.WaitForPlanetEscapeMusic, EndingCreditsPhase.WaitForPlanetEscapeMusicQueue, EndingCreditsPhase.PlanetEscapeFast, EndingCreditsPhase.PlanetEscapeSlow, EndingCreditsPhase.PlanetEscapeAccelerating, EndingCreditsPhase.OperationSuccessfulText, EndingCreditsPhase.FadeOutToCredits],
            None with { FloorScene = EndingFloorSceneId.PlanetExplosion, FlyawayFloor = true, ExplosionMaps = true }),
        ([EndingCreditsPhase.Credits, EndingCreditsPhase.PostCreditsBlank, EndingCreditsPhase.PostCreditsFadeIn, EndingCreditsPhase.PostCreditsShootingStars, EndingCreditsPhase.PostCreditsWaitingBackdrop, EndingCreditsPhase.PostCreditsWaitingSamus, EndingCreditsPhase.PostCreditsReward, EndingCreditsPhase.PostCreditsCopyright, EndingCreditsPhase.PostCreditsGesture, EndingCreditsPhase.PostCreditsJump],
            None with { PostCreditsArt = true }),
        ([EndingCreditsPhase.PostCreditsShot],
            None with { PostCreditsArt = true, RewardIcon = true }),
        ([EndingCreditsPhase.PostCreditsWhiteFlash, EndingCreditsPhase.PostCreditsLogo, EndingCreditsPhase.ItemPercentage, EndingCreditsPhase.ItemPercentageScrollDown, EndingCreditsPhase.SeeYouNextMission],
            None with { PostCreditsArt = true, RewardIcon = true, PostShotParts = PostShotPartCount }),
    ];
}
