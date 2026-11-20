namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The order the ending runs its phases in, for checks that walk or compare them (#2571).</summary>
internal static class EndingCreditsPhaseOrder
{
    /// <summary>Every phase, in the order the ending runs them.</summary>
    internal static IReadOnlyList<EndingCreditsPhase> All { get; } =
    [
        EndingCreditsPhase.SetupEscapeFromZebes,
        EndingCreditsPhase.WaitForEscapeMusic,
        EndingCreditsPhase.FadeInSideCloudEscape,
        EndingCreditsPhase.SideCloudEscape,
        EndingCreditsPhase.FadeOutSideCloudEscape,
        EndingCreditsPhase.FadeInTopAndBottomCloudEscape,
        EndingCreditsPhase.TopAndBottomCloudEscape,
        EndingCreditsPhase.FadeOutTopAndBottomCloudEscape,
        EndingCreditsPhase.FadeInZebesExplosion,
        EndingCreditsPhase.ZebesExplosionCrossfade,
        EndingCreditsPhase.ZebesExplosionHold,
        EndingCreditsPhase.ZebesExplosionAnimation,
        EndingCreditsPhase.WaitForPlanetEscapeMusic,
        EndingCreditsPhase.WaitForPlanetEscapeMusicQueue,
        EndingCreditsPhase.PlanetEscapeFast,
        EndingCreditsPhase.PlanetEscapeSlow,
        EndingCreditsPhase.PlanetEscapeAccelerating,
        EndingCreditsPhase.OperationSuccessfulText,
        EndingCreditsPhase.FadeOutToCredits,
        EndingCreditsPhase.Credits,
        EndingCreditsPhase.PostCreditsBlank,
        EndingCreditsPhase.PostCreditsFadeIn,
        EndingCreditsPhase.PostCreditsShootingStars,
        EndingCreditsPhase.PostCreditsWaitingBackdrop,
        EndingCreditsPhase.PostCreditsWaitingSamus,
        EndingCreditsPhase.PostCreditsReward,
        EndingCreditsPhase.PostCreditsCopyright,
        EndingCreditsPhase.PostCreditsGesture,
        EndingCreditsPhase.PostCreditsJump,
        EndingCreditsPhase.PostCreditsShot,
        EndingCreditsPhase.PostCreditsWhiteFlash,
        EndingCreditsPhase.PostCreditsLogo,
        EndingCreditsPhase.ItemPercentage,
        EndingCreditsPhase.ItemPercentageScrollDown,
        EndingCreditsPhase.SeeYouNextMission,
    ];

    /// <summary>Whether the ending runs <paramref name="phase"/> before <paramref name="later"/>.</summary>
    internal static bool IsBefore(EndingCreditsPhase phase, EndingCreditsPhase later) =>
        Position(phase) < Position(later);

    /// <summary>Where <paramref name="phase"/> runs among the ending's phases.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The phase is not one of the ending's.</exception>
    private static int Position(EndingCreditsPhase phase)
    {
        int position = All.ToList().IndexOf(phase);
        return position >= 0 ? position : throw new ArgumentOutOfRangeException(nameof(phase), phase, "Not one of the ending's phases.");
    }
}
