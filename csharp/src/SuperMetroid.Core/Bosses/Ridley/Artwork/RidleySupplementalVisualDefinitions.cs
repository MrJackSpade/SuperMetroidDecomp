using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Fixed wing and articulated-tail visual identities selected by Ridley's
/// wing and tail draws. Pose cadence and segment motion remain gameplay state.
/// </summary>
internal static class RidleySupplementalVisualDefinitions
{

    /// <summary>
    /// The wing beat facing left (#1935): the wings' downstroke from fully raised to fully lowered, then the upstroke
    /// through the four poses between.
    /// </summary>
    internal static IReadOnlyList<EnemyFrameId> WingBeatFacingLeft { get; } =
    [
        EnemyFrameId.RidleyWingFacingLeftFirst, EnemyFrameId.RidleyWingFacingLeftSecond, EnemyFrameId.RidleyWingFacingLeftThird,
        EnemyFrameId.RidleyWingFacingLeftFourth, EnemyFrameId.RidleyWingFacingLeftFifth, EnemyFrameId.RidleyWingFacingLeftSixth,
        EnemyFrameId.RidleyWingFacingLeftFifth, EnemyFrameId.RidleyWingFacingLeftFourth, EnemyFrameId.RidleyWingFacingLeftThird,
        EnemyFrameId.RidleyWingFacingLeftSecond,
    ];

    /// <summary>The wing beat facing right, step for step the mirror of <see cref="WingBeatFacingLeft"/>.</summary>
    internal static IReadOnlyList<EnemyFrameId> WingBeatFacingRight { get; } =
    [
        EnemyFrameId.RidleyWingFacingRightFirst, EnemyFrameId.RidleyWingFacingRightSecond, EnemyFrameId.RidleyWingFacingRightThird,
        EnemyFrameId.RidleyWingFacingRightFourth, EnemyFrameId.RidleyWingFacingRightFifth, EnemyFrameId.RidleyWingFacingRightSixth,
        EnemyFrameId.RidleyWingFacingRightFifth, EnemyFrameId.RidleyWingFacingRightFourth, EnemyFrameId.RidleyWingFacingRightThird,
        EnemyFrameId.RidleyWingFacingRightSecond,
    ];

    /// <summary>The steps of a wing beat, the same facing either way.</summary>
    internal static int WingBeatSteps => WingBeatFacingLeft.Count;

    /// <summary>
    /// The installed frame the drawn tail tip shows for each sixteenth of the turn its direction can point into, in turn
    /// order: each frame's sixteenth is centred on it, so the first opens half a frame before straight down.
    /// </summary>
    internal static TurnArcs<EnemyFrameId> TailTipFrames { get; } = new(TrigonometryAngle.Wrapped(-8),
        EnemyFrameId.RidleyBreakupTailTipFirst, EnemyFrameId.RidleyBreakupTailTipSecond, EnemyFrameId.RidleyBreakupTailTipThird,
        EnemyFrameId.RidleyBreakupTailTipFourth, EnemyFrameId.RidleyBreakupTailTipFifth, EnemyFrameId.RidleyBreakupTailTipSixth,
        EnemyFrameId.RidleyBreakupTailTipSeventh, EnemyFrameId.RidleyBreakupTailTipEighth, EnemyFrameId.RidleyBreakupTailTipNinth,
        EnemyFrameId.RidleyBreakupTailTipTenth, EnemyFrameId.RidleyBreakupTailTipEleventh, EnemyFrameId.RidleyBreakupTailTipTwelfth,
        EnemyFrameId.RidleyBreakupTailTipThirteenth, EnemyFrameId.RidleyBreakupTailTipFourteenth, EnemyFrameId.RidleyBreakupTailTipFifteenth,
        EnemyFrameId.RidleyBreakupTailTipSixteenth);

    /// <summary>The installed frame of the two tail links nearest Ridley's body.</summary>
    internal static EnemyFrameId BaseLink => EnemyFrameId.RidleyTailBaseLink;

    /// <summary>The installed frame of the two middle tail links.</summary>
    internal static EnemyFrameId MiddleLink => EnemyFrameId.RidleyTailMiddleLink;

    /// <summary>The installed frame of the two tail links nearest the tip.</summary>
    internal static EnemyFrameId OuterLink => EnemyFrameId.RidleyTailOuterLink;

    /// <summary>The installed frame each of the six drawn tail links shows, from the body out (#1935).</summary>
    internal static IReadOnlyList<EnemyFrameId> LinkFrames { get; } = [BaseLink, BaseLink, MiddleLink, MiddleLink, OuterLink, OuterLink];
}
