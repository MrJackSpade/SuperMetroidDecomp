using System.Collections.Frozen;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The stretches of the ending the presentation tests (#1452): each names the phases it covers instead of ordering the
/// phase identities.
/// </summary>
internal static class EndingCreditsPhaseGroups
{
    /// <summary>The escape from Zebes, before the planet's explosion fades in.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> BeforeZebesExplosion = new EndingCreditsPhase[]
    {
        EndingCreditsPhase.SetupEscapeFromZebes, EndingCreditsPhase.WaitForEscapeMusic, EndingCreditsPhase.FadeInSideCloudEscape,
        EndingCreditsPhase.SideCloudEscape, EndingCreditsPhase.FadeOutSideCloudEscape, EndingCreditsPhase.FadeInTopAndBottomCloudEscape,
        EndingCreditsPhase.TopAndBottomCloudEscape, EndingCreditsPhase.FadeOutTopAndBottomCloudEscape,
    }.ToFrozenSet();

    /// <summary>The escape and the explosion's fade-in and crossfade, before the explosion's hold.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> BeforeZebesExplosionHold = BeforeZebesExplosion.Concat(new EndingCreditsPhase[]
    {
        EndingCreditsPhase.FadeInZebesExplosion, EndingCreditsPhase.ZebesExplosionCrossfade,
    }).ToFrozenSet();

    /// <summary>Everything before the gunship's planet escape: the escape, the explosion and the music waits.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> BeforePlanetEscape = BeforeZebesExplosionHold.Concat(new EndingCreditsPhase[]
    {
        EndingCreditsPhase.ZebesExplosionHold, EndingCreditsPhase.ZebesExplosionAnimation,
        EndingCreditsPhase.WaitForPlanetEscapeMusic, EndingCreditsPhase.WaitForPlanetEscapeMusicQueue,
    }).ToFrozenSet();

    /// <summary>The gunship's planet escape through the fade out to the credits.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> PlanetEscape = new EndingCreditsPhase[]
    {
        EndingCreditsPhase.PlanetEscapeFast, EndingCreditsPhase.PlanetEscapeSlow, EndingCreditsPhase.PlanetEscapeAccelerating,
        EndingCreditsPhase.OperationSuccessfulText, EndingCreditsPhase.FadeOutToCredits,
    }.ToFrozenSet();

    /// <summary>The post-credits scenes from the waiting Samus onward.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> FromWaitingSamus = new EndingCreditsPhase[]
    {
        EndingCreditsPhase.PostCreditsWaitingSamus, EndingCreditsPhase.PostCreditsReward, EndingCreditsPhase.PostCreditsCopyright,
        EndingCreditsPhase.PostCreditsGesture, EndingCreditsPhase.PostCreditsJump, EndingCreditsPhase.PostCreditsShot,
        EndingCreditsPhase.PostCreditsWhiteFlash, EndingCreditsPhase.PostCreditsLogo, EndingCreditsPhase.ItemPercentage,
        EndingCreditsPhase.ItemPercentageScrollDown, EndingCreditsPhase.SeeYouNextMission,
    }.ToFrozenSet();

    /// <summary>The planet escape's music waits, which render a white screen.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> Whiteout =
        new[] { EndingCreditsPhase.WaitForPlanetEscapeMusic, EndingCreditsPhase.WaitForPlanetEscapeMusicQueue }.ToFrozenSet();

    /// <summary>The post-credits phases that draw no actors or stars: the copyright, the waiting Samus and the blank screen.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> HidingSprites =
        new[] { EndingCreditsPhase.PostCreditsCopyright, EndingCreditsPhase.PostCreditsWaitingSamus, EndingCreditsPhase.PostCreditsBlank }.ToFrozenSet();

    /// <summary>The post-credits phases that show no background: the blank screen, the fade in, the gesture and the jump.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> HidingPostCreditsBackground = new[]
    {
        EndingCreditsPhase.PostCreditsBlank, EndingCreditsPhase.PostCreditsFadeIn, EndingCreditsPhase.PostCreditsGesture,
        EndingCreditsPhase.PostCreditsJump,
    }.ToFrozenSet();

    /// <summary>The phases that reveal the reward, whose background and glow follow the reward's reveal.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> RewardReveal = new[] { EndingCreditsPhase.PostCreditsReward }.ToFrozenSet();

    /// <summary>The phases whose sprites glow over the background and the sky: the explosion's crossfade and the shooting stars.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> GlowingSprites = new[]
    {
        EndingCreditsPhase.FadeInZebesExplosion, EndingCreditsPhase.ZebesExplosionCrossfade, EndingCreditsPhase.PostCreditsShootingStars,
    }.ToFrozenSet();

    /// <summary>The phases that start the gunship's fast planet escape, which sets the waiting starfield moving.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> StartingFlyaway = new[] { EndingCreditsPhase.PlanetEscapeFast }.ToFrozenSet();

    /// <summary>The phases that start the completion text, which the planet escape's afterglow and starfields leave for.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> CompletionText = new[] { EndingCreditsPhase.OperationSuccessfulText }.ToFrozenSet();

    /// <summary>The credits' final message, which the completed game is recorded on reaching.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> FinalMessage = new[] { EndingCreditsPhase.SeeYouNextMission }.ToFrozenSet();

    /// <summary>Everything after the credits roll.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> PostCredits = new EndingCreditsPhase[]
    {
        EndingCreditsPhase.PostCreditsBlank, EndingCreditsPhase.PostCreditsFadeIn, EndingCreditsPhase.PostCreditsShootingStars,
        EndingCreditsPhase.PostCreditsWaitingBackdrop,
    }.Concat(FromWaitingSamus).ToFrozenSet();
}
