using P = SuperMetroid.Core.Enemies.Animals.DachoraSegment;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Dachora's run, idle, blink, shinespark charge, echo and fall animations (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and no beat raises a cue for its owner.
/// </summary>
internal static class DachoraAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<DachoraSegment, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<DachoraSegment, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.RunningLeft] =
        [
            Show(EnemyFrameId.DachoraRunningLeftFirst, 5),
            Show(EnemyFrameId.DachoraRunningLeftSecond, 5),
            Show(EnemyFrameId.DachoraRunningLeftThird, 5),
            Show(EnemyFrameId.DachoraRunningLeftFourth, 5),
            Show(EnemyFrameId.DachoraRunningLeftFifth, 5),
            Show(EnemyFrameId.DachoraRunningLeftSixth, 5),
        ],
        [P.RunningLeftFast] =
        [
            Show(EnemyFrameId.DachoraRunningLeftFirst, 3),
            Show(EnemyFrameId.DachoraRunningLeftSecond, 3),
            Show(EnemyFrameId.DachoraRunningLeftThird, 3),
            Show(EnemyFrameId.DachoraRunningLeftFourth, 3),
            Show(EnemyFrameId.DachoraRunningLeftFifth, 3),
            Show(EnemyFrameId.DachoraRunningLeftSixth, 3),
        ],
        [P.RunningLeftVeryFast] =
        [
            Show(EnemyFrameId.DachoraRunningLeftFirst, 1),
            Show(EnemyFrameId.DachoraRunningLeftSecond, 1),
            Show(EnemyFrameId.DachoraRunningLeftThird, 1),
            Show(EnemyFrameId.DachoraRunningLeftFourth, 1),
            Show(EnemyFrameId.DachoraRunningLeftFifth, 1),
            Show(EnemyFrameId.DachoraRunningLeftSixth, 1),
        ],
        [P.IdleLeft] =
        [
            Show(EnemyFrameId.DachoraIdleLeftFirst, 48),
            Show(EnemyFrameId.DachoraIdleLeftSecond, 10),
            Show(EnemyFrameId.DachoraIdleLeftThird, 7),
            Show(EnemyFrameId.DachoraIdleLeftFourth, 7),
            Show(EnemyFrameId.DachoraIdleLeftFifth, 7),
            Show(EnemyFrameId.DachoraIdleLeftFourth, 7),
            Show(EnemyFrameId.DachoraIdleLeftThird, 7),
            Show(EnemyFrameId.DachoraIdleLeftFourth, 7),
            Show(EnemyFrameId.DachoraIdleLeftFifth, 7),
            Show(EnemyFrameId.DachoraIdleLeftFourth, 7),
            Show(EnemyFrameId.DachoraIdleLeftSecond, 10),
        ],
        [P.BlinkLeft] =
        [
            Show(EnemyFrameId.DachoraIdleLeftFirst, 11),
            Show(EnemyFrameId.DachoraBlinkLeftSecond, 8),
            Show(EnemyFrameId.DachoraBlinkLeftThird, 8),
            Show(EnemyFrameId.DachoraIdleLeftFirst, 4),
            Show(EnemyFrameId.DachoraBlinkLeftSecond, 4),
            Show(EnemyFrameId.DachoraBlinkLeftThird, 4),
            Show(EnemyFrameId.DachoraIdleLeftFirst, 10),
            Show(EnemyFrameId.DachoraBlinkLeftSecond, 5),
            Show(EnemyFrameId.DachoraBlinkLeftThird, 11),
        ],
        [P.EchoLeft] =
        [
            Show(EnemyFrameId.DachoraEchoLeft, 10),
        ],
        [P.FallingLeft] =
        [
            Show(EnemyFrameId.DachoraRunningLeftFifth, 5),
        ],
        [P.RunningRight] =
        [
            Show(EnemyFrameId.DachoraRunningRightFirst, 5),
            Show(EnemyFrameId.DachoraRunningRightSecond, 5),
            Show(EnemyFrameId.DachoraRunningRightThird, 5),
            Show(EnemyFrameId.DachoraRunningRightFourth, 5),
            Show(EnemyFrameId.DachoraRunningRightFifth, 5),
            Show(EnemyFrameId.DachoraRunningRightSixth, 5),
        ],
        [P.RunningRightFast] =
        [
            Show(EnemyFrameId.DachoraRunningRightFirst, 3),
            Show(EnemyFrameId.DachoraRunningRightSecond, 3),
            Show(EnemyFrameId.DachoraRunningRightThird, 3),
            Show(EnemyFrameId.DachoraRunningRightFourth, 3),
            Show(EnemyFrameId.DachoraRunningRightFifth, 3),
            Show(EnemyFrameId.DachoraRunningRightSixth, 3),
        ],
        [P.RunningRightVeryFast] =
        [
            Show(EnemyFrameId.DachoraRunningRightFirst, 1),
            Show(EnemyFrameId.DachoraRunningRightSecond, 1),
            Show(EnemyFrameId.DachoraRunningRightThird, 1),
            Show(EnemyFrameId.DachoraRunningRightFourth, 1),
            Show(EnemyFrameId.DachoraRunningRightFifth, 1),
            Show(EnemyFrameId.DachoraRunningRightSixth, 1),
        ],
        [P.IdleRight] =
        [
            Show(EnemyFrameId.DachoraIdleRightFirst, 48),
            Show(EnemyFrameId.DachoraIdleRightSecond, 10),
            Show(EnemyFrameId.DachoraIdleRightThird, 7),
            Show(EnemyFrameId.DachoraIdleRightFourth, 7),
            Show(EnemyFrameId.DachoraIdleRightFifth, 7),
            Show(EnemyFrameId.DachoraIdleRightFourth, 7),
            Show(EnemyFrameId.DachoraIdleRightThird, 7),
            Show(EnemyFrameId.DachoraIdleRightFourth, 7),
            Show(EnemyFrameId.DachoraIdleRightFifth, 7),
            Show(EnemyFrameId.DachoraIdleRightFourth, 7),
            Show(EnemyFrameId.DachoraIdleRightSecond, 10),
        ],
        [P.BlinkRight] =
        [
            Show(EnemyFrameId.DachoraIdleRightFirst, 11),
            Show(EnemyFrameId.DachoraBlinkRightSecond, 8),
            Show(EnemyFrameId.DachoraBlinkRightThird, 8),
            Show(EnemyFrameId.DachoraIdleRightFirst, 4),
            Show(EnemyFrameId.DachoraBlinkRightSecond, 4),
            Show(EnemyFrameId.DachoraBlinkRightThird, 4),
            Show(EnemyFrameId.DachoraIdleRightFirst, 10),
            Show(EnemyFrameId.DachoraBlinkRightSecond, 5),
            Show(EnemyFrameId.DachoraBlinkRightThird, 11),
        ],
        [P.ChargeRight] =
        [
            Show(EnemyFrameId.DachoraChargeRight, 1),
        ],
        [P.EchoRight] =
        [
            Show(EnemyFrameId.DachoraEchoRight, 10),
        ],
        [P.FallingRight] =
        [
            Show(EnemyFrameId.DachoraRunningRightFifth, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<DachoraSegment, Animation<DachoraSegment>> StartingAt { get; } = new Dictionary<DachoraSegment, Animation<DachoraSegment>>
    {
        [P.RunningLeft] = new([new(P.RunningLeft, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningLeftFast] = new([new(P.RunningLeftFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningLeftVeryFast] = new([new(P.RunningLeftVeryFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IdleLeft] = new([new(P.IdleLeft, 0, 11, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BlinkLeft] = new([new(P.BlinkLeft, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.EchoLeft] = new([new(P.EchoLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FallingLeft] = new([new(P.FallingLeft, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRight] = new([new(P.RunningRight, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightFast] = new([new(P.RunningRightFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RunningRightVeryFast] = new([new(P.RunningRightVeryFast, 0, 6, 1)], AnimationEnd.LoopsFrom(0)),
        [P.IdleRight] = new([new(P.IdleRight, 0, 11, 1)], AnimationEnd.LoopsFrom(0)),
        [P.BlinkRight] = new([new(P.BlinkRight, 0, 9, 1)], AnimationEnd.LoopsFrom(0)),
        [P.ChargeRight] = new([new(P.ChargeRight, 0, 1, 1)], AnimationEnd.Holds),
        [P.EchoRight] = new([new(P.EchoRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.FallingRight] = new([new(P.FallingRight, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<DachoraSegment> StartingWith(DachoraSegment segment) => StartingAt[segment];

    /// <summary>The segment a sleeping animation resumes into when its owner wakes it.</summary>
    internal static DachoraSegment AfterSleep(DachoraSegment segment) => segment switch
    {
        P.ChargeRight => P.EchoRight,
        P.RunningLeft or
            P.RunningLeftFast or
            P.RunningLeftVeryFast or
            P.IdleLeft or
            P.BlinkLeft or
            P.EchoLeft or
            P.FallingLeft or
            P.RunningRight or
            P.RunningRightFast or
            P.RunningRightVeryFast or
            P.IdleRight or
            P.BlinkRight or
            P.EchoRight or
            P.FallingRight => throw new ArgumentOutOfRangeException(nameof(segment), segment, "The animation does not sleep."),
        _ => throw new ArgumentOutOfRangeException(nameof(segment), segment, "The animation does not sleep."),
    };

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
