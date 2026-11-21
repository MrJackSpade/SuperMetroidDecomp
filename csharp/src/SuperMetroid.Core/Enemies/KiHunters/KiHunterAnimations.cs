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
        [P.FlyingLeft] = Hover(EnemyFrameId.KiHunterHoveringFacingLeft, EnemyFrameId.KiHunterHoveringFacingLeftDropped),
        [P.SwoopLeft] = Swoop(EnemyFrameId.KiHunterSwoopingFacingLeftArmsBack, EnemyFrameId.KiHunterSwoopingFacingLeftArmsIn, EnemyFrameId.KiHunterHoveringFacingLeft, EnemyFrameId.KiHunterSwoopingFacingLeftLegsReaching, EnemyFrameId.KiHunterSwoopingFacingLeftLegsStretched),
        [P.FlyingRight] = Hover(EnemyFrameId.KiHunterHoveringFacingRight, EnemyFrameId.KiHunterHoveringFacingRightDropped),
        [P.SwoopRight] = Swoop(EnemyFrameId.KiHunterSwoopingFacingRightArmsBack, EnemyFrameId.KiHunterSwoopingFacingRightArmsIn, EnemyFrameId.KiHunterHoveringFacingRight, EnemyFrameId.KiHunterSwoopingFacingRightLegsReaching, EnemyFrameId.KiHunterSwoopingFacingRightLegsStretched),
        [P.WingsLeft] = Flap(EnemyFrameId.KiHunterWingsFacingLeftRaised, EnemyFrameId.KiHunterWingsFacingLeftMid, EnemyFrameId.KiHunterWingsFacingLeftLevel),
        [P.WingsRight] = Flap(EnemyFrameId.KiHunterWingsFacingRightRaised, EnemyFrameId.KiHunterWingsFacingRightMid, EnemyFrameId.KiHunterWingsFacingRightLevel),
        [P.DetachedWings] =
        [
            Hold(EnemyFrameId.KiHunterDetachedWings),
        ],
        [P.JumpLeft] = Hop(EnemyFrameId.KiHunterCrouchedFacingLeft, EnemyFrameId.KiHunterCrouchingLowFacingLeft, EnemyFrameId.KiHunterSpringingFacingLeft, EnemyFrameId.KiHunterLeapingFacingLeft),
        [P.JumpRight] = Hop(EnemyFrameId.KiHunterCrouchedFacingRight, EnemyFrameId.KiHunterCrouchingLowFacingRight, EnemyFrameId.KiHunterSpringingFacingRight, EnemyFrameId.KiHunterLeapingFacingRight),
        [P.LandLeft] = Landing(EnemyFrameId.KiHunterCrouchedFacingLeft, EnemyFrameId.KiHunterCrouchingLowFacingLeft, EnemyFrameId.KiHunterSpringingFacingLeft),
        [P.LandRight] = Landing(EnemyFrameId.KiHunterCrouchedFacingRight, EnemyFrameId.KiHunterCrouchingLowFacingRight, EnemyFrameId.KiHunterSpringingFacingRight),
        [P.SpitLeft] = Spit(EnemyFrameId.KiHunterCrouchedFacingLeft, EnemyFrameId.KiHunterSpittingFacingLeftAcidGathering, EnemyFrameId.KiHunterSpittingFacingLeftAcidSwollen, EnemyFrameId.KiHunterSpittingFacingLeftAcidSpat, HorizontalDirection.Left, EnemyFrameId.KiHunterSpittingFacingLeftAcidSpent),
        [P.SpitRight] = Spit(EnemyFrameId.KiHunterCrouchedFacingRight, EnemyFrameId.KiHunterSpittingFacingRightAcidGathering, EnemyFrameId.KiHunterSpittingFacingRightAcidSwollen, EnemyFrameId.KiHunterSpittingFacingRightAcidSpat, HorizontalDirection.Right, EnemyFrameId.KiHunterSpittingFacingRightAcidSpent),
    });

    /// <summary>
    /// The animation that starts at each segment an owner can start or restart at. A flight ends at its return-to-flying
    /// cue, which always restarts the flight for the body's direction (#2061). A hop, a landing and a spit end on their
    /// last pose held (#2439): the cue before it hands the body to its next behaviour, which shows its own picture.
    /// </summary>
    private static Dictionary<KiHunterSegment, Animation<KiHunterSegment>> StartingAt { get; } = new Dictionary<KiHunterSegment, Animation<KiHunterSegment>>
    {
        [P.FlyingLeft] = new([Segments.Whole(P.FlyingLeft)], AnimationEnd.Holds),
        [P.SwoopLeft] = new([Segments.Whole(P.SwoopLeft), Segments.Whole(P.FlyingLeft)], AnimationEnd.Holds),
        [P.FlyingRight] = new([Segments.Whole(P.FlyingRight)], AnimationEnd.Holds),
        [P.SwoopRight] = new([Segments.Whole(P.SwoopRight), Segments.Whole(P.FlyingRight)], AnimationEnd.Holds),
        [P.WingsLeft] = Animation<KiHunterSegment>.Looping([Segments.Whole(P.WingsLeft)]),
        [P.WingsRight] = Animation<KiHunterSegment>.Looping([Segments.Whole(P.WingsRight)]),
        [P.DetachedWings] = new([Segments.Whole(P.DetachedWings)], AnimationEnd.Holds),
        [P.JumpLeft] = new([Segments.Whole(P.JumpLeft)], AnimationEnd.Holds),
        [P.JumpRight] = new([Segments.Whole(P.JumpRight)], AnimationEnd.Holds),
        [P.LandLeft] = new([Segments.Whole(P.LandLeft)], AnimationEnd.Holds),
        [P.LandRight] = new([Segments.Whole(P.LandRight)], AnimationEnd.Holds),
        [P.SpitLeft] = new([Segments.Whole(P.SpitLeft)], AnimationEnd.Holds),
        [P.SpitRight] = new([Segments.Whole(P.SpitRight)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KiHunterSegment> StartingWith(KiHunterSegment segment) => StartingAt[segment];

    /// <summary>A Ki-Hunter hovering over a facing's poses, <paramref name="hunterHovering"/>, <paramref name="dropped"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue>[] Hover(EnemyFrameId hunterHovering, EnemyFrameId dropped) =>
    [
        Show(hunterHovering, 2),
        Show(dropped, 2),
        Show(hunterHovering, 1),
        Cue(new KiHunterCue.Signal(K.ReturnToFlying)),
    ];

    /// <summary>A Ki-Hunter's swoop over a facing's poses, <paramref name="armsBack"/>, <paramref name="armsIn"/>, <paramref name="hunterHovering"/>, <paramref name="legsReaching"/>, <paramref name="legsStretched"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue>[] Swoop(EnemyFrameId armsBack, EnemyFrameId armsIn, EnemyFrameId hunterHovering, EnemyFrameId legsReaching, EnemyFrameId legsStretched) =>
    [
        Show(armsBack, 2),
        Show(armsIn, 6),
        Show(armsBack, 2),
        Show(hunterHovering, 2),
        Show(legsReaching, 2),
        Show(legsStretched, 32),
    ];

    /// <summary>A Ki-Hunter's wings flapping over a facing's poses, <paramref name="raised"/>, <paramref name="mid"/>, <paramref name="level"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue>[] Flap(EnemyFrameId raised, EnemyFrameId mid, EnemyFrameId level) =>
    [
        Show(raised, 2),
        Show(mid, 2),
        Show(level, 1),
    ];

    /// <summary>A wingless Ki-Hunter's hop over a facing's poses, <paramref name="hunterCrouched"/>, <paramref name="hunterCrouchingLow"/>, <paramref name="hunterSpringing"/>, <paramref name="hunterLeaping"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue>[] Hop(EnemyFrameId hunterCrouched, EnemyFrameId hunterCrouchingLow, EnemyFrameId hunterSpringing, EnemyFrameId hunterLeaping) =>
    [
        Show(hunterCrouched, 8),
        Show(hunterCrouchingLow, 8),
        Show(hunterSpringing, 11),
        Show(hunterCrouchingLow, 2),
        Show(hunterCrouched, 2),
        Cue(new KiHunterCue.Signal(K.StartHop)),
        Hold(hunterLeaping),
    ];

    /// <summary>A wingless Ki-Hunter's landing over a facing's poses, <paramref name="hunterCrouched"/>, <paramref name="hunterCrouchingLow"/>, <paramref name="hunterSpringing"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue>[] Landing(EnemyFrameId hunterCrouched, EnemyFrameId hunterCrouchingLow, EnemyFrameId hunterSpringing) =>
    [
        Show(hunterCrouched, 8),
        Show(hunterCrouchingLow, 8),
        Show(hunterSpringing, 11),
        Show(hunterCrouchingLow, 8),
        Cue(new KiHunterCue.Signal(K.StartWinglessThinking)),
        Hold(hunterCrouched),
    ];

    /// <summary>A Ki-Hunter's acid spit over a facing's poses, <paramref name="hunterCrouched"/>, <paramref name="acidGathering"/>, <paramref name="acidSwollen"/>, <paramref name="acidSpat"/>, <paramref name="facing"/>, <paramref name="acidSpent"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue>[] Spit(EnemyFrameId hunterCrouched, EnemyFrameId acidGathering, EnemyFrameId acidSwollen, EnemyFrameId acidSpat, HorizontalDirection facing, EnemyFrameId acidSpent) =>
    [
        Show(hunterCrouched, 32),
        Show(acidGathering, 6),
        Show(acidSwollen, 16),
        Show(acidSpat, 2),
        Cue(new KiHunterCue.Spit(facing)),
        Show(acidSpent, 24),
        Cue(new KiHunterCue.Signal(K.StartWinglessThinking)),
        Hold(hunterCrouched),
    ];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, KiHunterCue>.Frame(frame, duration);

    /// <summary>Shows <paramref name="frame"/> for good (#2337): a still picture rests on it until its owner plays another.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue> Hold(EnemyFrameId frame) => new AnimationBeat<EnemyFrameId, KiHunterCue>.HeldFrame(frame);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, KiHunterCue> Cue(KiHunterCue cue) => new AnimationBeat<EnemyFrameId, KiHunterCue>.Event(cue);
}
