using P = SuperMetroid.Core.Enemies.Sbug.SbugProgram;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Sbug's eight directional animation loops as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and its lists use no owner callbacks.
/// </summary>
internal static class SbugAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SbugProgram, EnemyFrameId, NoAnimationCue> Segments { get; } = new(
        new Dictionary<SbugProgram, AnimationBeat<EnemyFrameId, NoAnimationCue>[]>
    {
        [P.Right] =
        [
            Show(EnemyFrameId.SbugRight0, 5),
            Show(EnemyFrameId.SbugRight1, 5),
            Show(EnemyFrameId.SbugRight0, 5),
            Show(EnemyFrameId.SbugRight2, 5),
        ],
        [P.UpRight] =
        [
            Show(EnemyFrameId.SbugUpRight0, 5),
            Show(EnemyFrameId.SbugUpRight1, 5),
            Show(EnemyFrameId.SbugUpRight0, 5),
            Show(EnemyFrameId.SbugUpRight2, 5),
        ],
        [P.Up] =
        [
            Show(EnemyFrameId.SbugUp0, 5),
            Show(EnemyFrameId.SbugUp1, 5),
            Show(EnemyFrameId.SbugUp0, 5),
            Show(EnemyFrameId.SbugUp2, 5),
        ],
        [P.UpLeft] =
        [
            Show(EnemyFrameId.SbugUpLeft0, 5),
            Show(EnemyFrameId.SbugUpLeft1, 5),
            Show(EnemyFrameId.SbugUpLeft0, 5),
            Show(EnemyFrameId.SbugUpLeft2, 5),
        ],
        [P.Left] =
        [
            Show(EnemyFrameId.SbugLeft0, 5),
            Show(EnemyFrameId.SbugLeft1, 5),
            Show(EnemyFrameId.SbugLeft0, 5),
            Show(EnemyFrameId.SbugLeft2, 5),
        ],
        [P.DownLeft] =
        [
            Show(EnemyFrameId.SbugDownLeft0, 5),
            Show(EnemyFrameId.SbugDownLeft1, 5),
            Show(EnemyFrameId.SbugDownLeft0, 5),
            Show(EnemyFrameId.SbugDownLeft2, 5),
        ],
        [P.Down] =
        [
            Show(EnemyFrameId.SbugDown0, 5),
            Show(EnemyFrameId.SbugDown1, 5),
            Show(EnemyFrameId.SbugDown0, 5),
            Show(EnemyFrameId.SbugDown2, 5),
        ],
        [P.DownRight] =
        [
            Show(EnemyFrameId.SbugDownRight0, 5),
            Show(EnemyFrameId.SbugDownRight1, 5),
            Show(EnemyFrameId.SbugDownRight0, 5),
            Show(EnemyFrameId.SbugDownRight2, 5),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SbugProgram, Animation<SbugProgram>> StartingAt { get; } = new Dictionary<SbugProgram, Animation<SbugProgram>>
    {
        [P.Right] = new([new(P.Right, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpRight] = new([new(P.UpRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Up] = new([new(P.Up, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.UpLeft] = new([new(P.UpLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Left] = new([new(P.Left, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownLeft] = new([new(P.DownLeft, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Down] = new([new(P.Down, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
        [P.DownRight] = new([new(P.DownRight, 0, 4, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SbugProgram> StartingWith(SbugProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, NoAnimationCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, NoAnimationCue>.Frame(frame, duration);
}
