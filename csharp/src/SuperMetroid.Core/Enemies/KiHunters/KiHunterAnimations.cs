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
            Show(EnemyFrameId.KiHunterHoveringFacingLeftTwin, 1),
            Cue(new(K.ReturnToFlying)),
        ],
        [P.SwoopLeft] =
        [
            Show(EnemyFrameId.KiHunterSwoopingFacingLeftArmsBack, 2),
            Show(EnemyFrameId.KiHunterSwoopingFacingLeftArmsIn, 6),
            Show(EnemyFrameId.KiHunterSwoopingFacingLeftArmsBack, 2),
            Show(EnemyFrameId.KiHunterHoveringFacingLeftTwinB, 2),
            Show(EnemyFrameId.KiHunterSwoopingFacingLeftLegsReaching, 2),
            Show(EnemyFrameId.KiHunterSwoopingFacingLeftLegsStretched, 32),
        ],
        [P.FlyingRight] =
        [
            Show(EnemyFrameId.KiHunterHoveringFacingRight, 2),
            Show(EnemyFrameId.KiHunterHoveringFacingRightDropped, 2),
            Show(EnemyFrameId.KiHunterHoveringFacingRightTwin, 1),
            Cue(new(K.ReturnToFlying)),
        ],
        [P.SwoopRight] =
        [
            Show(EnemyFrameId.KiHunterSwoopingFacingRightArmsBack, 2),
            Show(EnemyFrameId.KiHunterSwoopingFacingRightArmsIn, 6),
            Show(EnemyFrameId.KiHunterSwoopingFacingRightArmsBack, 2),
            Show(EnemyFrameId.KiHunterHoveringFacingRightTwinB, 2),
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
            Show(EnemyFrameId.KiHunterDetachedWings, 1),
        ],
        [P.JumpLeft] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingLeft, 8),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingLeft, 8),
            Show(EnemyFrameId.KiHunterSpringingFacingLeft, 11),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingLeft, 2),
            Show(EnemyFrameId.KiHunterCrouchedFacingLeft, 2),
            Cue(new(K.StartHop)),
            Show(EnemyFrameId.KiHunterLeapingFacingLeft, 1),
        ],
        [P.JumpRight] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingRight, 8),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingRight, 8),
            Show(EnemyFrameId.KiHunterSpringingFacingRight, 11),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingRight, 2),
            Show(EnemyFrameId.KiHunterCrouchedFacingRight, 2),
            Cue(new(K.StartHop)),
            Show(EnemyFrameId.KiHunterLeapingFacingRight, 1),
        ],
        [P.LandLeft] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingLeft, 8),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingLeft, 8),
            Show(EnemyFrameId.KiHunterSpringingFacingLeft, 11),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingLeft, 8),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterCrouchedFacingLeft, 1),
        ],
        [P.LandRight] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingRight, 8),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingRight, 8),
            Show(EnemyFrameId.KiHunterSpringingFacingRight, 11),
            Show(EnemyFrameId.KiHunterCrouchingLowFacingRight, 8),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterCrouchedFacingRight, 1),
        ],
        [P.SpitLeft] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingLeftTwin, 32),
            Show(EnemyFrameId.KiHunterSpittingFacingLeftAcidGathering, 6),
            Show(EnemyFrameId.KiHunterSpittingFacingLeftAcidSwollen, 16),
            Show(EnemyFrameId.KiHunterSpittingFacingLeftAcidSpat, 2),
            Cue(new(K.SpitLeft)),
            Show(EnemyFrameId.KiHunterSpittingFacingLeftAcidSpent, 24),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterCrouchedFacingLeftTwin, 1),
        ],
        [P.SpitRight] =
        [
            Show(EnemyFrameId.KiHunterCrouchedFacingRightTwin, 32),
            Show(EnemyFrameId.KiHunterSpittingFacingRightAcidGathering, 6),
            Show(EnemyFrameId.KiHunterSpittingFacingRightAcidSwollen, 16),
            Show(EnemyFrameId.KiHunterSpittingFacingRightAcidSpat, 2),
            Cue(new(K.SpitRight)),
            Show(EnemyFrameId.KiHunterSpittingFacingRightAcidSpent, 24),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterCrouchedFacingRightTwin, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KiHunterSegment, Animation<KiHunterSegment>> StartingAt { get; } = new Dictionary<KiHunterSegment, Animation<KiHunterSegment>>
    {
        [P.FlyingLeft] = new([new(P.FlyingLeft, 0, 4, 1), new(P.SwoopLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwoopLeft] = new([new(P.SwoopLeft, 0, 6, 1), new(P.FlyingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FlyingRight] = new([new(P.FlyingRight, 0, 4, 1), new(P.SwoopRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwoopRight] = new([new(P.SwoopRight, 0, 6, 1), new(P.FlyingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WingsLeft] = new([new(P.WingsLeft, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.WingsRight] = new([new(P.WingsRight, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DetachedWings] = new([new(P.DetachedWings, 0, 1, 1)], AnimationEnd.Holds),
        [P.JumpLeft] = new([new(P.JumpLeft, 0, 7, 1)], AnimationEnd.Holds),
        [P.JumpRight] = new([new(P.JumpRight, 0, 7, 1)], AnimationEnd.Holds),
        [P.LandLeft] = new([new(P.LandLeft, 0, 6, 1)], AnimationEnd.Holds),
        [P.LandRight] = new([new(P.LandRight, 0, 6, 1)], AnimationEnd.Holds),
        [P.SpitLeft] = new([new(P.SpitLeft, 0, 8, 1)], AnimationEnd.Holds),
        [P.SpitRight] = new([new(P.SpitRight, 0, 8, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KiHunterSegment> StartingWith(KiHunterSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, KiHunterCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue> Cue(KiHunterCue cue) => new AnimationBeat<EnemyFrameId, KiHunterCue>.Event(cue);
}
