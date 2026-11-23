using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The statue a <see cref="TourianStatueBlockCue"/> acts on (#2571): each operation carries out one cue's effect with the
/// statue's definition, and each condition returns the animation the statue starts when it holds, or null.
/// </summary>
internal interface ITourianStatueCueTarget
{
    /// <summary>Makes the statue join the statues that are animating.</summary>
    void MarkAnimating();

    /// <summary>The already-grey finish when the statue's grey event exists, else null.</summary>
    AnimationContinuation<TourianStatueBlockSegment>? MayAlreadyBeGrey();

    /// <summary>The release when the statue's area boss is dead, else null.</summary>
    AnimationContinuation<TourianStatueBlockSegment>? MayBeReleased();

    /// <summary>Makes the statue stop animating.</summary>
    void ClearAnimating();

    /// <summary>The waiting oscillation while another release runs, else null.</summary>
    AnimationContinuation<TourianStatueBlockSegment>? MayWaitItsTurn();

    /// <summary>Claims the release for this statue.</summary>
    void ClaimRelease();

    /// <summary>Shows the statue's eye colours blacked out.</summary>
    void BlackOutEyes();

    /// <summary>Spawns the glow of the statue's eye.</summary>
    void SpawnEyeGlow();

    /// <summary>Spawns the soul the statue releases.</summary>
    void SpawnSoul();

    /// <summary>Starts the colour effect that greys the statue.</summary>
    void StartGreyOut();

    /// <summary>Sets the statue's grey event.</summary>
    void SetGreyEvent();

    /// <summary>Finishes the statue's release.</summary>
    void FinishRelease();

    /// <summary>Shows the statue grey.</summary>
    void TurnGrey();
}
