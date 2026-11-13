using P = SuperMetroid.Core.Enemies.Ripper.RipperProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// The GRipper, Ripper II and Ripper flying loops as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class RipperAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RipperProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<RipperProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.GRipperMovingLeft] =
        [
            Show(EnemyFrameId.RipperSharedLeft0, 8),
            Show(EnemyFrameId.RipperSharedLeft1, 7),
            Show(EnemyFrameId.RipperSharedLeft0, 8),
            Show(EnemyFrameId.RipperSharedLeft2, 7),
        ],
        [P.GRipperMovingRight] =
        [
            Show(EnemyFrameId.RipperSharedRight0, 8),
            Show(EnemyFrameId.RipperSharedRight1, 7),
            Show(EnemyFrameId.RipperSharedRight0, 8),
            Show(EnemyFrameId.RipperSharedRight2, 7),
        ],
        [P.Ripper2MovingRight] =
        [
            Show(EnemyFrameId.RipperSharedLeft0, 8),
            Show(EnemyFrameId.RipperSharedLeft1, 7),
            Show(EnemyFrameId.RipperSharedLeft0, 8),
            Show(EnemyFrameId.RipperSharedLeft2, 7),
        ],
        [P.Ripper2MovingLeft] =
        [
            Show(EnemyFrameId.RipperSharedRight0, 8),
            Show(EnemyFrameId.RipperSharedRight1, 7),
            Show(EnemyFrameId.RipperSharedRight0, 8),
            Show(EnemyFrameId.RipperSharedRight2, 7),
        ],
        [P.RipperMovingRight] =
        [
            Show(EnemyFrameId.RipperRight0, 8),
            Show(EnemyFrameId.RipperRight1, 7),
            Show(EnemyFrameId.RipperRight0, 8),
            Show(EnemyFrameId.RipperRight2, 7),
        ],
        [P.RipperMovingLeft] =
        [
            Show(EnemyFrameId.RipperLeft0, 8),
            Show(EnemyFrameId.RipperLeft1, 7),
            Show(EnemyFrameId.RipperLeft0, 8),
            Show(EnemyFrameId.RipperLeft2, 7),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<RipperProgram, Animation<RipperProgram>> StartingAt { get; } = new Dictionary<RipperProgram, Animation<RipperProgram>>
    {
        [P.GRipperMovingLeft] = new([new(P.GRipperMovingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.GRipperMovingRight] = new([new(P.GRipperMovingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Ripper2MovingRight] = new([new(P.Ripper2MovingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Ripper2MovingLeft] = new([new(P.Ripper2MovingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RipperMovingRight] = new([new(P.RipperMovingRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.RipperMovingLeft] = new([new(P.RipperMovingLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RipperProgram> StartingWith(RipperProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
