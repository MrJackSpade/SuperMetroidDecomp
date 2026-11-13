using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Rios.LowerNorfairRioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.LowerNorfairRioProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Lower Norfair Rio's (Holtz's) idle, swoop and cooldown animations and its flames' animation as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="LowerNorfairRioCue"/> with its typed operands.
/// </summary>
internal static class LowerNorfairRioAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<LowerNorfairRioProgram, EnemyFrameId, LowerNorfairRioCue> Segments { get; } = new(
        new Dictionary<LowerNorfairRioProgram, AnimationBeat<EnemyFrameId, LowerNorfairRioCue>[]>
    {
        [P.Idle] =
        [
            Cue(new(K.HideFlames)),
        ],
        [P.IdleLoop] =
        [
            Show(EnemyFrameId.LowerNorfairRioPose0, 11),
            Show(EnemyFrameId.LowerNorfairRioPose1, 11),
            Show(EnemyFrameId.LowerNorfairRioPose2, 11),
            Show(EnemyFrameId.LowerNorfairRioPose1, 11),
        ],
        [P.PrepareToSwoop] =
        [
            Cue(new(K.HideFlames)),
            Show(EnemyFrameId.LowerNorfairRioPose2, 3),
            Show(EnemyFrameId.LowerNorfairRioPose3, 3),
            Show(EnemyFrameId.LowerNorfairRioPose4, 3),
            Show(EnemyFrameId.LowerNorfairRioPose5, 3),
            Show(EnemyFrameId.LowerNorfairRioPose6, 2),
            Show(EnemyFrameId.LowerNorfairRioPose7, 1),
            Show(EnemyFrameId.LowerNorfairRioPose8, 2),
            Show(EnemyFrameId.LowerNorfairRioPose9, 3),
            Show(EnemyFrameId.LowerNorfairRioPose10, 3),
            Cue(new(K.AnimationFinished)),
        ],
        [P.Descending] =
        [
            Cue(new(K.HideFlames)),
            Show(EnemyFrameId.LowerNorfairRioPose10, 1),
        ],
        [P.AscendingPart1] =
        [
            Cue(new(K.HideFlames)),
            Show(EnemyFrameId.LowerNorfairRioPose10, 3),
            Show(EnemyFrameId.LowerNorfairRioPose9, 3),
            Show(EnemyFrameId.LowerNorfairRioPose11, 3),
            Cue(new(K.AnimationFinished)),
        ],
        [P.AscendingPart2] =
        [
            Cue(new(K.ShowFlames)),
        ],
        [P.AscendingPart2Loop] =
        [
            Show(EnemyFrameId.LowerNorfairRioPose14, 2),
            Show(EnemyFrameId.LowerNorfairRioPose12, 2),
            Show(EnemyFrameId.LowerNorfairRioPose13, 2),
        ],
        [P.Cooldown] =
        [
            Cue(new(K.ShowFlames)),
            Show(EnemyFrameId.LowerNorfairRioPose11, 3),
            Show(EnemyFrameId.LowerNorfairRioPose9, 3),
            Show(EnemyFrameId.LowerNorfairRioPose8, 2),
            Show(EnemyFrameId.LowerNorfairRioPose7, 1),
            Show(EnemyFrameId.LowerNorfairRioPose6, 2),
            Show(EnemyFrameId.LowerNorfairRioPose5, 3),
            Show(EnemyFrameId.LowerNorfairRioPose4, 1),
            Show(EnemyFrameId.LowerNorfairRioPose3, 1),
            Show(EnemyFrameId.LowerNorfairRioPose2, 1),
            Cue(new(K.AnimationFinished)),
        ],
        [P.Flames] =
        [
            Show(EnemyFrameId.LowerNorfairRioPose15, 6),
            Show(EnemyFrameId.LowerNorfairRioPose16, 4),
            Show(EnemyFrameId.LowerNorfairRioPose17, 3),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<LowerNorfairRioProgram, Animation<LowerNorfairRioProgram>> StartingAt { get; } = new Dictionary<LowerNorfairRioProgram, Animation<LowerNorfairRioProgram>>
    {
        [P.Idle] = new([new(P.Idle, 0, 1, 1), new(P.IdleLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(1)),
        [P.IdleLoop] = new([new(P.IdleLoop, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.PrepareToSwoop] = new([new(P.PrepareToSwoop, 0, 11, 1)], AnimationEnd.Holds),
        [P.Descending] = new([new(P.Descending, 0, 2, 1)], AnimationEnd.Holds),
        [P.AscendingPart1] = new([new(P.AscendingPart1, 0, 5, 1)], AnimationEnd.Holds),
        [P.AscendingPart2] = new([new(P.AscendingPart2, 0, 1, 1), new(P.AscendingPart2Loop, 0, 3, 1)], AnimationEnd.LoopsFrom(1)),
        [P.AscendingPart2Loop] = new([new(P.AscendingPart2Loop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Cooldown] = new([new(P.Cooldown, 0, 11, 1)], AnimationEnd.Holds),
        [P.Flames] = new([new(P.Flames, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<LowerNorfairRioProgram> StartingWith(LowerNorfairRioProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, LowerNorfairRioCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, LowerNorfairRioCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, LowerNorfairRioCue> Cue(LowerNorfairRioCue cue) => new AnimationBeat<EnemyFrameId, LowerNorfairRioCue>.Event(cue);
}
