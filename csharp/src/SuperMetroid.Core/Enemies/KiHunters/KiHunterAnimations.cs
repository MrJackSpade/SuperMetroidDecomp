using SuperMetroid.Core.Animation;
using K = SuperMetroid.Core.Enemies.KiHunters.KiHunterCueKind;
using P = SuperMetroid.Core.Enemies.KiHunters.KiHunterSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter body's flight, swoop, hop, landing and spit animations and its wings' animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each cue is a <see cref="KiHunterCue"/> naming what it does.
/// </summary>
internal static class KiHunterAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KiHunterSegment, EnemyFrameId, KiHunterCue> Segments { get; } = new(
        new Dictionary<KiHunterSegment, AnimationBeat<EnemyFrameId, KiHunterCue>[]>
    {
        [P.FlyingLeft] =
        [
            Show(EnemyFrameId.KiHunterHoveringFacingLeft, 2),
            Show(EnemyFrameId.KiHunterHoveringFacingLeftDropped, 2),
            Show(EnemyFrameId.KiHunterHoveringFacingLeft, 1),
            Cue(new KiHunterCue.Signal(K.ReturnToFlying)),
        ],
        [P.SwoopLeft] =
        [
            Show(EnemyFrameId.KiHunterSwoopingFacingLeftArmsBack, 2),
            Show(EnemyFrameId.KiHunterSwoopingFacingLeftArmsIn, 6),
            Show(EnemyFrameId.KiHunterSwoopingFacingLeftArmsBack, 2),
            Show(EnemyFrameId.KiHunterHoveringFacingLeft, 2),
            Show(EnemyFrameId.KiHunterSwoopingFacingLeftLegsReaching, 2),
            Show(EnemyFrameId.KiHunterSwoopingFacingLeftLegsStretched, 32),
        ],
        [P.FlyingRight] =
        [
            Show(EnemyFrameId.KiHunterHoveringFacingRight, 2),
            Show(EnemyFrameId.KiHunterHoveringFacingRightDropped, 2),
            Show(EnemyFrameId.KiHunterHoveringFacingRight, 1),
            Cue(new KiHunterCue.Signal(K.ReturnToFlying)),
        ],
        [P.SwoopRight] =
        [
            Show(EnemyFrameId.KiHunterSwoopingFacingRightArmsBack, 2),
            Show(EnemyFrameId.KiHunterSwoopingFacingRightArmsIn, 6),
            Show(EnemyFrameId.KiHunterSwoopingFacingRightArmsBack, 2),
            Show(EnemyFrameId.KiHunterHoveringFacingRight, 2),
            Show(EnemyFrameId.KiHunterSwoopingFacingRightLegsReaching, 2),
            Show(EnemyFrameId.KiHunterSwoopingFacingRightLegsStretched, 32),
        ],
        [P.WingsLeft] =
        [
            Show(EnemyFrameId.KiHunterWingsFacingLeftRaised, 2),
            Show(EnemyFrameId.KiHunterWingsFacingLeftMid, 2),
            Show(EnemyFrameId.KiHunterWingsFacingLeftLevel, 1),
        ],
        [P.WingsRight] =
        [
            Show(EnemyFrameId.KiHunterWingsFacingRightRaised, 2),
            Show(EnemyFrameId.KiHunterWingsFacingRightMid, 2),
            Show(EnemyFrameId.KiHunterWingsFacingRightLevel, 1),
        ],
        [P.DetachedWings] =
        [
            Hold(EnemyFrameId.KiHunterDetachedWings),
        ],
        [P.JumpLeft] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingLeft, 8),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingLeft, 8),
            Show(EnemyFrameId.KiHunterSpringingFacingLeft, 11),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingLeft, 2),
            Show(EnemyFrameId.KiHunterCrouchedFacingLeft, 2),
            Cue(new KiHunterCue.Signal(K.StartHop)),
            Show(EnemyFrameId.KiHunterLeapingFacingLeft, 1),
        ],
        [P.JumpRight] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingRight, 8),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingRight, 8),
            Show(EnemyFrameId.KiHunterSpringingFacingRight, 11),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingRight, 2),
            Show(EnemyFrameId.KiHunterCrouchedFacingRight, 2),
            Cue(new KiHunterCue.Signal(K.StartHop)),
            Show(EnemyFrameId.KiHunterLeapingFacingRight, 1),
        ],
        [P.LandLeft] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingLeft, 8),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingLeft, 8),
            Show(EnemyFrameId.KiHunterSpringingFacingLeft, 11),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingLeft, 8),
            Cue(new KiHunterCue.Signal(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterCrouchedFacingLeft, 1),
        ],
        [P.LandRight] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingRight, 8),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingRight, 8),
            Show(EnemyFrameId.KiHunterSpringingFacingRight, 11),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingRight, 8),
            Cue(new KiHunterCue.Signal(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterCrouchedFacingRight, 1),
        ],
        [P.SpitLeft] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingLeft, 32),
            Show(EnemyFrameId.KiHunterSpittingFacingLeftAcidGathering, 6),
            Show(EnemyFrameId.KiHunterSpittingFacingLeftAcidSwollen, 16),
            Show(EnemyFrameId.KiHunterSpittingFacingLeftAcidSpat, 2),
            Cue(new KiHunterCue.Spit(HorizontalDirection.Left)),
            Show(EnemyFrameId.KiHunterSpittingFacingLeftAcidSpent, 24),
            Cue(new KiHunterCue.Signal(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterCrouchedFacingLeft, 1),
        ],
        [P.SpitRight] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingRight, 32),
            Show(EnemyFrameId.KiHunterSpittingFacingRightAcidGathering, 6),
            Show(EnemyFrameId.KiHunterSpittingFacingRightAcidSwollen, 16),
            Show(EnemyFrameId.KiHunterSpittingFacingRightAcidSpat, 2),
            Cue(new KiHunterCue.Spit(HorizontalDirection.Right)),
            Show(EnemyFrameId.KiHunterSpittingFacingRightAcidSpent, 24),
            Cue(new KiHunterCue.Signal(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterCrouchedFacingRight, 1),
        ],
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. A flight ends at its return-to-flying
    /// cue, which always restarts the flight for the body's direction (#2061).
    /// </summary>
    internal static IReadOnlyDictionary<KiHunterSegment, Animation<KiHunterSegment>> StartingAt { get; } = new Dictionary<KiHunterSegment, Animation<KiHunterSegment>>
    {
        [P.FlyingLeft] = new([Whole(P.FlyingLeft)], AnimationEnd.Holds),
        [P.SwoopLeft] = new([Whole(P.SwoopLeft), Whole(P.FlyingLeft)], AnimationEnd.Holds),
        [P.FlyingRight] = new([Whole(P.FlyingRight)], AnimationEnd.Holds),
        [P.SwoopRight] = new([Whole(P.SwoopRight), Whole(P.FlyingRight)], AnimationEnd.Holds),
        [P.WingsLeft] = new([Whole(P.WingsLeft)], AnimationEnd.LoopsFrom(0)),
        [P.WingsRight] = new([Whole(P.WingsRight)], AnimationEnd.LoopsFrom(0)),
        [P.DetachedWings] = new([Whole(P.DetachedWings)], AnimationEnd.Holds),
        [P.JumpLeft] = new([Whole(P.JumpLeft)], AnimationEnd.Holds),
        [P.JumpRight] = new([Whole(P.JumpRight)], AnimationEnd.Holds),
        [P.LandLeft] = new([Whole(P.LandLeft)], AnimationEnd.Holds),
        [P.LandRight] = new([Whole(P.LandRight)], AnimationEnd.Holds),
        [P.SpitLeft] = new([Whole(P.SpitLeft)], AnimationEnd.Holds),
        [P.SpitRight] = new([Whole(P.SpitRight)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KiHunterSegment> StartingWith(KiHunterSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<KiHunterSegment> Whole(KiHunterSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, KiHunterCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, KiHunterCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue> Cue(KiHunterCue cue) => new AnimationBeat<EnemyFrameId, KiHunterCue>.Event(cue);
}
