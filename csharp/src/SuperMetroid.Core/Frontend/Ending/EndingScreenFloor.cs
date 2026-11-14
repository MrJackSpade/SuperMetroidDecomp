using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's turning floor (#1404, #1467, #1515): the installed picture of the shot the ending's phase shows.
/// </summary>
internal static class EndingScreenFloor
{
    /// <summary>The floor <paramref name="shot"/> shows.</summary>
    /// <exception cref="InvalidOperationException">The shot shows no floor.</exception>
    /// <exception cref="InvalidDataException">The installed floor sheet has no picture of the shot.</exception>
    internal static AffineFloor Of(InstalledSpriteSheets sheets, EndingFloorShot shot) => shot == EndingFloorShot.None
        ? throw new InvalidOperationException("The ending shows its floor in a phase that shows no floor.")
        : FloorShots.Of(sheets, EndingFloorDefinitions.Sheets[shot], EndingFloorDefinitions.FrameName);

    /// <summary>
    /// The shot the turning floor shows in <paramref name="phase"/>: each escape scene's clouds from its setup through its
    /// fade-out, the exploding planet from its fade-in through its hold, the gunship flying away from the explosion's
    /// animation through the fade to the credits, the reward icon while the reward's shot turns into the camera, and nothing
    /// before the first scene, through the credits and the post-credits scenes that show no floor.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The phase is undefined.</exception>
    internal static EndingFloorShot ShotOf(EndingCreditsPhase phase) => phase switch
    {
        EndingCreditsPhase.WaitForEscapeMusic or EndingCreditsPhase.FadeInSideCloudEscape or EndingCreditsPhase.SideCloudEscape
            or EndingCreditsPhase.FadeOutSideCloudEscape => EndingFloorShot.SideCloudEscape,
        EndingCreditsPhase.FadeInTopAndBottomCloudEscape or EndingCreditsPhase.TopAndBottomCloudEscape or EndingCreditsPhase.FadeOutTopAndBottomCloudEscape => EndingFloorShot.TopAndBottomCloudEscape,
        EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionPaletteCrossfade
            or EndingCreditsPhase.ZebesExplosionHold => EndingFloorShot.PlanetExplosion,
        EndingCreditsPhase.ZebesExplosionAnimation or EndingCreditsPhase.WaitForPlanetEscapeMusic
            or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue or EndingCreditsPhase.PlanetEscapeFast
            or EndingCreditsPhase.PlanetEscapeSlow or EndingCreditsPhase.PlanetEscapeAccelerating
            or EndingCreditsPhase.OperationSuccessfulText or EndingCreditsPhase.FadeOutToCredits => EndingFloorShot.Flyaway,
        EndingCreditsPhase.PostCreditsShot => EndingFloorShot.RewardIcon,
        EndingCreditsPhase.SetupEscapeFromZebes or EndingCreditsPhase.Credits or EndingCreditsPhase.PostCreditsBlank
            or EndingCreditsPhase.PostCreditsFadeIn or EndingCreditsPhase.PostCreditsShootingStars
            or EndingCreditsPhase.PostCreditsWaitingBackdrop or EndingCreditsPhase.PostCreditsWaitingSamus
            or EndingCreditsPhase.PostCreditsReward or EndingCreditsPhase.PostCreditsCopyright
            or EndingCreditsPhase.PostCreditsGesture or EndingCreditsPhase.PostCreditsJump
            or EndingCreditsPhase.PostCreditsWhiteFlash or EndingCreditsPhase.PostCreditsLogo
            or EndingCreditsPhase.ItemPercentage or EndingCreditsPhase.ItemPercentageScrollDown
            or EndingCreditsPhase.SeeYouNextMission => EndingFloorShot.None,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined ending phase."),
    };
}
