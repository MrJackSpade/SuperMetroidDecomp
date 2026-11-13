using P = SuperMetroid.Core.Enemies.Bull.BullProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Bull's normal and immune-shot animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class BullAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<BullProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<BullProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Normal] =
        [
            Show(EnemyFrameId.BullIdle0, 10),
            Show(EnemyFrameId.BullIdle1, 10),
            Show(EnemyFrameId.BullIdle0, 10),
            Show(EnemyFrameId.BullIdle2, 10),
        ],
        [P.Shot] =
        [
        ],
        [P.ShotLoop] =
        [
            Show(EnemyFrameId.BullIdle0, 3),
            Show(EnemyFrameId.BullIdle1, 3),
            Show(EnemyFrameId.BullIdle0, 3),
            Show(EnemyFrameId.BullIdle2, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<BullProgram, Animation<BullProgram>> StartingAt { get; } = new Dictionary<BullProgram, Animation<BullProgram>>
    {
        [P.Normal] = new([new(P.Normal, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Shot] = new([new(P.ShotLoop, 0, 4, 5), new(P.Normal, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<BullProgram> StartingWith(BullProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
