using P = SuperMetroid.Core.Enemies.Boulder.BoulderProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Boulder;

/// <summary>
/// The Boulder's mirrored rolling loops as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class BoulderAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BoulderProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BoulderProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Left] =
        [
            Show(EnemyFrameId.BoulderRoll0, 8),
            Show(EnemyFrameId.BoulderRoll1, 8),
            Show(EnemyFrameId.BoulderRoll2, 8),
            Show(EnemyFrameId.BoulderRoll3, 8),
            Show(EnemyFrameId.BoulderRoll4, 8),
            Show(EnemyFrameId.BoulderRoll5, 8),
            Show(EnemyFrameId.BoulderRoll6, 8),
            Show(EnemyFrameId.BoulderRoll7, 8),
        ],
        [P.Right] =
        [
            Show(EnemyFrameId.BoulderRoll0, 8),
            Show(EnemyFrameId.BoulderRoll7, 8),
            Show(EnemyFrameId.BoulderRoll6, 8),
            Show(EnemyFrameId.BoulderRoll5, 8),
            Show(EnemyFrameId.BoulderRoll4, 8),
            Show(EnemyFrameId.BoulderRoll3, 8),
            Show(EnemyFrameId.BoulderRoll2, 8),
            Show(EnemyFrameId.BoulderRoll1, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BoulderProgram, Animation<BoulderProgram>> StartingAt { get; } = new Dictionary<BoulderProgram, Animation<BoulderProgram>>
    {
        [P.Left] = new([new(P.Left, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Right] = new([new(P.Right, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BoulderProgram> StartingWith(BoulderProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
