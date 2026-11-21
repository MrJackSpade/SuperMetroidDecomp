using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Fixed wing and articulated-tail visual identities selected by Ridley's
/// wing and tail draws. Pose cadence and segment motion remain gameplay state.
/// </summary>
internal static class RidleySupplementalVisualDefinitions
{

    /// <summary>The wings fully raised, where the downstroke starts.</summary>
    internal static RidleyWingPose WingsRaised { get; } = new(EnemyFrameId.RidleyWingFacingLeftRaised, EnemyFrameId.RidleyWingFacingRightRaised);

    /// <summary>The wings lowering from fully raised.</summary>
    internal static RidleyWingPose WingsLowering { get; } = new(EnemyFrameId.RidleyWingFacingLeftLowering, EnemyFrameId.RidleyWingFacingRightLowering);

    /// <summary>The wings level, seen edge on.</summary>
    internal static RidleyWingPose WingsEdgeOn { get; } = new(EnemyFrameId.RidleyWingFacingLeftEdgeOn, EnemyFrameId.RidleyWingFacingRightEdgeOn);

    /// <summary>The wings just below level, tilted edge on.</summary>
    internal static RidleyWingPose WingsEdgeOnTilted { get; } = new(EnemyFrameId.RidleyWingFacingLeftEdgeOnTilted, EnemyFrameId.RidleyWingFacingRightEdgeOnTilted);

    /// <summary>The wings dipping toward the bottom of the stroke.</summary>
    internal static RidleyWingPose WingsDipping { get; } = new(EnemyFrameId.RidleyWingFacingLeftDipping, EnemyFrameId.RidleyWingFacingRightDipping);

    /// <summary>The wings fully lowered, where the upstroke starts.</summary>
    internal static RidleyWingPose WingsDown { get; } = new(EnemyFrameId.RidleyWingFacingLeftDown, EnemyFrameId.RidleyWingFacingRightDown);

    /// <summary>
    /// The wing beat (#1935, #2329): the wings' downstroke from fully raised to fully lowered, then the upstroke through
    /// the four poses between, after which the beat starts again.
    /// </summary>
    internal static IReadOnlyList<RidleyWingPose> WingBeat { get; } =
    [
        WingsRaised, WingsLowering, WingsEdgeOn, WingsEdgeOnTilted, WingsDipping,
        WingsDown, WingsDipping, WingsEdgeOnTilted, WingsEdgeOn, WingsLowering,
    ];

    /// <summary>
    /// The installed frame the drawn tail tip shows for each sixteenth of the turn its direction can point into, in turn
    /// order: each frame's sixteenth is centred on it, so the first opens half a frame before straight down.
    /// </summary>
    internal static TurnArcs<EnemyFrameId> TailTipFrames { get; } = new(TrigonometryAngle.Wrapped(-8),
        EnemyFrameId.RidleyTailTipPointingToward180, EnemyFrameId.RidleyTailTipPointingToward157, EnemyFrameId.RidleyTailTipPointingToward135,
        EnemyFrameId.RidleyTailTipPointingToward112, EnemyFrameId.RidleyTailTipPointingToward90, EnemyFrameId.RidleyTailTipPointingToward67,
        EnemyFrameId.RidleyTailTipPointingToward45, EnemyFrameId.RidleyTailTipPointingToward22, EnemyFrameId.RidleyTailTipPointingToward0,
        EnemyFrameId.RidleyTailTipPointingToward337, EnemyFrameId.RidleyTailTipPointingToward315, EnemyFrameId.RidleyTailTipPointingToward292,
        EnemyFrameId.RidleyTailTipPointingToward270, EnemyFrameId.RidleyTailTipPointingToward247, EnemyFrameId.RidleyTailTipPointingToward225,
        EnemyFrameId.RidleyTailTipPointingToward202);

    /// <summary>The installed frame of the two tail links nearest Ridley's body.</summary>
    internal static EnemyFrameId BaseLink => EnemyFrameId.RidleyTailBaseLink;

    /// <summary>The installed frame of the two middle tail links.</summary>
    internal static EnemyFrameId MiddleLink => EnemyFrameId.RidleyTailMiddleLink;

    /// <summary>The installed frame of the two tail links nearest the tip.</summary>
    internal static EnemyFrameId OuterLink => EnemyFrameId.RidleyTailOuterLink;

    /// <summary>The installed frames the drawn tail links show (#1935); each link's definition names its own.</summary>
    internal static IReadOnlyList<EnemyFrameId> LinkFrames { get; } = [BaseLink, MiddleLink, OuterLink];
}
