using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.KiHunters.KiHunterCueKind;
using P = SuperMetroid.Core.Enemies.KiHunters.KiHunterSegment;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter body's flight, swoop, hop, landing and spit animations and its wings' animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="KiHunterCue"/> with its typed operands.
/// </summary>
internal static class KiHunterAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KiHunterSegment, EnemyFrameId, KiHunterCue> Segments { get; } = new(
        new Dictionary<KiHunterSegment, AnimationBeat<EnemyFrameId, KiHunterCue>[]>
    {
        [P.FlyingLeft] =
        [
            Show(EnemyFrameId.KiHunterFlyingLeftFirst, 2),
            Show(EnemyFrameId.KiHunterFlyingLeftSecond, 2),
            Show(EnemyFrameId.KiHunterFlyingLeftThird, 1),
            Cue(new(K.ReturnToFlying)),
        ],
        [P.SwoopLeft] =
        [
            Show(EnemyFrameId.KiHunterSwoopLeftFirst, 2),
            Show(EnemyFrameId.KiHunterSwoopLeftSecond, 6),
            Show(EnemyFrameId.KiHunterSwoopLeftFirst, 2),
            Show(EnemyFrameId.KiHunterSwoopLeftThird, 2),
            Show(EnemyFrameId.KiHunterSwoopLeftFourth, 2),
            Show(EnemyFrameId.KiHunterSwoopLeftFifth, 32),
        ],
        [P.FlyingRight] =
        [
            Show(EnemyFrameId.KiHunterFlyingRightFirst, 2),
            Show(EnemyFrameId.KiHunterFlyingRightSecond, 2),
            Show(EnemyFrameId.KiHunterFlyingRightThird, 1),
            Cue(new(K.ReturnToFlying)),
        ],
        [P.SwoopRight] =
        [
            Show(EnemyFrameId.KiHunterSwoopRightFirst, 2),
            Show(EnemyFrameId.KiHunterSwoopRightSecond, 6),
            Show(EnemyFrameId.KiHunterSwoopRightFirst, 2),
            Show(EnemyFrameId.KiHunterSwoopRightThird, 2),
            Show(EnemyFrameId.KiHunterSwoopRightFourth, 2),
            Show(EnemyFrameId.KiHunterSwoopRightFifth, 32),
        ],
        [P.WingsLeft] =
        [
            Show(EnemyFrameId.KiHunterWingsLeftFirst, 2),
            Show(EnemyFrameId.KiHunterWingsLeftSecond, 2),
            Show(EnemyFrameId.KiHunterWingsLeftThird, 1),
        ],
        [P.WingsRight] =
        [
            Show(EnemyFrameId.KiHunterWingsRightFirst, 2),
            Show(EnemyFrameId.KiHunterWingsRightSecond, 2),
            Show(EnemyFrameId.KiHunterWingsRightThird, 1),
        ],
        [P.DetachedWings] =
        [
            Show(EnemyFrameId.KiHunterDetachedWings, 1),
        ],
        [P.JumpLeft] =
        [
            Show(EnemyFrameId.KiHunterJumpLeftFirst, 8),
            Show(EnemyFrameId.KiHunterJumpLeftSecond, 8),
            Show(EnemyFrameId.KiHunterJumpLeftThird, 11),
            Show(EnemyFrameId.KiHunterJumpLeftSecond, 2),
            Show(EnemyFrameId.KiHunterJumpLeftFirst, 2),
            Cue(new(K.StartHop)),
            Show(EnemyFrameId.KiHunterJumpLeftFourth, 1),
        ],
        [P.JumpRight] =
        [
            Show(EnemyFrameId.KiHunterJumpRightFirst, 8),
            Show(EnemyFrameId.KiHunterJumpRightSecond, 8),
            Show(EnemyFrameId.KiHunterJumpRightThird, 11),
            Show(EnemyFrameId.KiHunterJumpRightSecond, 2),
            Show(EnemyFrameId.KiHunterJumpRightFirst, 2),
            Cue(new(K.StartHop)),
            Show(EnemyFrameId.KiHunterJumpRightFourth, 1),
        ],
        [P.LandLeft] =
        [
            Show(EnemyFrameId.KiHunterJumpLeftFirst, 8),
            Show(EnemyFrameId.KiHunterJumpLeftSecond, 8),
            Show(EnemyFrameId.KiHunterJumpLeftThird, 11),
            Show(EnemyFrameId.KiHunterJumpLeftSecond, 8),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterJumpLeftFirst, 1),
        ],
        [P.LandRight] =
        [
            Show(EnemyFrameId.KiHunterJumpRightFirst, 8),
            Show(EnemyFrameId.KiHunterJumpRightSecond, 8),
            Show(EnemyFrameId.KiHunterJumpRightThird, 11),
            Show(EnemyFrameId.KiHunterJumpRightSecond, 8),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterJumpRightFirst, 1),
        ],
        [P.SpitLeft] =
        [
            Show(EnemyFrameId.KiHunterSpitLeftFirst, 32),
            Show(EnemyFrameId.KiHunterSpitLeftSecond, 6),
            Show(EnemyFrameId.KiHunterSpitLeftThird, 16),
            Show(EnemyFrameId.KiHunterSpitLeftFourth, 2),
            Cue(new(K.SpitLeft)),
            Show(EnemyFrameId.KiHunterSpitLeftFifth, 24),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterSpitLeftFirst, 1),
        ],
        [P.SpitRight] =
        [
            Show(EnemyFrameId.KiHunterSpitRightFirst, 32),
            Show(EnemyFrameId.KiHunterSpitRightSecond, 6),
            Show(EnemyFrameId.KiHunterSpitRightThird, 16),
            Show(EnemyFrameId.KiHunterSpitRightFourth, 2),
            Cue(new(K.SpitRight)),
            Show(EnemyFrameId.KiHunterSpitRightFifth, 24),
            Cue(new(K.StartWinglessThinking)),
            Show(EnemyFrameId.KiHunterSpitRightFirst, 1),
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
