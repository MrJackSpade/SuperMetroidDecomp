using P = SuperMetroid.Core.Enemies.Puyo.PuyoProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// Puyo's grounded loops and airborne hop poses as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class PuyoAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<PuyoProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<PuyoProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.GroundedFast] =
        [
            Show(EnemyFrameId.PuyoGround0, 5),
            Show(EnemyFrameId.PuyoGround1, 5),
            Show(EnemyFrameId.PuyoGround2, 5),
            Show(EnemyFrameId.PuyoGround1, 5),
        ],
        [P.GroundedMedium] =
        [
            Show(EnemyFrameId.PuyoGround0, 8),
            Show(EnemyFrameId.PuyoGround1, 8),
            Show(EnemyFrameId.PuyoGround2, 8),
            Show(EnemyFrameId.PuyoGround1, 8),
        ],
        [P.GroundedSlow] =
        [
            Show(EnemyFrameId.PuyoGround0, 10),
            Show(EnemyFrameId.PuyoGround1, 10),
            Show(EnemyFrameId.PuyoGround2, 10),
            Show(EnemyFrameId.PuyoGround1, 10),
        ],
        [P.RightFrame0LeftFrame4] =
        [
            Show(EnemyFrameId.PuyoAir4, 1),
        ],
        [P.RightFrame1LeftFrame3] =
        [
            Show(EnemyFrameId.PuyoAir3, 1),
        ],
        [P.Frame2] =
        [
            Show(EnemyFrameId.PuyoAir2, 1),
        ],
        [P.RightFrame3LeftFrame1] =
        [
            Show(EnemyFrameId.PuyoAir1, 1),
        ],
        [P.RightFrame4LeftFrame0] =
        [
            Show(EnemyFrameId.PuyoAir0, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<PuyoProgram, Animation<PuyoProgram>> StartingAt { get; } = new Dictionary<PuyoProgram, Animation<PuyoProgram>>
    {
        [P.GroundedFast] = new([new(P.GroundedFast, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GroundedMedium] = new([new(P.GroundedMedium, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GroundedSlow] = new([new(P.GroundedSlow, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RightFrame0LeftFrame4] = new([new(P.RightFrame0LeftFrame4, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFrame1LeftFrame3] = new([new(P.RightFrame1LeftFrame3, 0, 1, 1)], AnimationEnd.Holds),
        [P.Frame2] = new([new(P.Frame2, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFrame3LeftFrame1] = new([new(P.RightFrame3LeftFrame1, 0, 1, 1)], AnimationEnd.Holds),
        [P.RightFrame4LeftFrame0] = new([new(P.RightFrame4LeftFrame0, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PuyoProgram> StartingWith(PuyoProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
