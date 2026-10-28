using System.Collections.Frozen;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The stretches of the ending the presentation tests (#1452): each names the phases it covers instead of ordering the
/// phase identities.
/// </summary>
internal static class EndingCreditsPhaseGroups
{
    /// <summary>The escape from Zebes, before the planet's explosion fades in.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> BeforeZebesExplosion = new[]
    {
        EndingCreditsPhase.SetupEscapeFromZebes, EndingCreditsPhase.WaitForEscapeMusic, EndingCreditsPhase.FadeInEscapeSceneA,
        EndingCreditsPhase.EscapeSceneA, EndingCreditsPhase.FadeOutEscapeSceneA, EndingCreditsPhase.FadeInEscapeSceneB,
        EndingCreditsPhase.EscapeSceneB, EndingCreditsPhase.FadeOutEscapeSceneB,
    }.ToFrozenSet();

    /// <summary>The escape and the explosion's fade-in and crossfade, before its tiles upload.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> BeforeZebesExplosionTileUpload = BeforeZebesExplosion.Concat(new[]
    {
        EndingCreditsPhase.FadeInZebesExplosion, EndingCreditsPhase.ZebesExplosionPaletteCrossfade,
    }).ToFrozenSet();

    /// <summary>Everything before the gunship's planet escape: the escape, the explosion and the music waits.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> BeforePlanetEscape = BeforeZebesExplosionTileUpload.Concat(new[]
    {
        EndingCreditsPhase.ZebesExplosionTileUpload, EndingCreditsPhase.ZebesExplosionAnimation,
        EndingCreditsPhase.WaitForPlanetEscapeMusic, EndingCreditsPhase.WaitForPlanetEscapeMusicQueue,
    }).ToFrozenSet();

    /// <summary>The gunship's planet escape through the fade out to the credits.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> PlanetEscape = new[]
    {
        EndingCreditsPhase.PlanetEscapeFast, EndingCreditsPhase.PlanetEscapeSlow, EndingCreditsPhase.PlanetEscapeAccelerating,
        EndingCreditsPhase.OperationSuccessfulText, EndingCreditsPhase.FadeOutToCredits,
    }.ToFrozenSet();

    /// <summary>The post-credits scenes from the waiting Samus onward.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> FromWaitingSamus = new[]
    {
        EndingCreditsPhase.PostCreditsWaitingSamus, EndingCreditsPhase.PostCreditsReward, EndingCreditsPhase.PostCreditsCopyright,
        EndingCreditsPhase.PostCreditsGesture, EndingCreditsPhase.PostCreditsJump, EndingCreditsPhase.PostCreditsShot,
        EndingCreditsPhase.PostCreditsWhiteFlash, EndingCreditsPhase.PostCreditsLogo, EndingCreditsPhase.ItemPercentage,
        EndingCreditsPhase.ItemPercentageScrollDown, EndingCreditsPhase.SeeYouNextMission,
    }.ToFrozenSet();

    /// <summary>Everything after the credits roll.</summary>
    internal static readonly FrozenSet<EndingCreditsPhase> PostCredits = new[]
    {
        EndingCreditsPhase.PostCreditsBlank, EndingCreditsPhase.PostCreditsFadeIn, EndingCreditsPhase.PostCreditsShootingStars,
        EndingCreditsPhase.PostCreditsWaitingBackdrop,
    }.Concat(FromWaitingSamus).ToFrozenSet();
}
