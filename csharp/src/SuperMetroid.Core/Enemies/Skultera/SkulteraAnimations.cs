using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Skultera.SkulteraCueKind;
using P = SuperMetroid.Core.Enemies.Skultera.SkulteraProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// Skultera's swimming and turning animations as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="SkulteraCue"/> with its typed operands.
/// </summary>
internal static class SkulteraAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SkulteraProgram, EnemyFrameId, SkulteraCue> Segments { get; } = new(
        new Dictionary<SkulteraProgram, AnimationBeat<EnemyFrameId, SkulteraCue>[]>
    {
        [P.SwimmingLeft] = [],
        [P.SwimmingLeftLoop] =
        [
            Show(EnemyFrameId.SkulteraSwimLeft0, 14),
            Show(EnemyFrameId.SkulteraSwimLeft1, 14),
            Show(EnemyFrameId.SkulteraSwimLeft2, 14),
        ],
        [P.TurningRight] =
        [
            Show(EnemyFrameId.SkulteraTurnRight0, 13),
            Show(EnemyFrameId.SkulteraTurnRight1, 10),
            Show(EnemyFrameId.SkulteraTurnRight2, 8),
            Show(EnemyFrameId.SkulteraTurnRight3, 6),
            Show(EnemyFrameId.SkulteraTurnRight4, 6),
            Show(EnemyFrameId.SkulteraTurnRight5, 8),
            Show(EnemyFrameId.SkulteraTurnRight6, 10),
            Show(EnemyFrameId.SkulteraTurnRight7, 13),
            Cue(new(K.FinishTurn)),
        ],
        [P.SwimmingRight] = [],
        [P.SwimmingRightLoop] =
        [
            Show(EnemyFrameId.SkulteraSwimRight0, 14),
            Show(EnemyFrameId.SkulteraSwimRight1, 14),
            Show(EnemyFrameId.SkulteraSwimRight2, 14),
        ],
        [P.TurningLeft] =
        [
            Show(EnemyFrameId.SkulteraTurnLeft0, 13),
            Show(EnemyFrameId.SkulteraTurnLeft1, 10),
            Show(EnemyFrameId.SkulteraTurnLeft2, 8),
            Show(EnemyFrameId.SkulteraTurnLeft3, 6),
            Show(EnemyFrameId.SkulteraTurnLeft4, 6),
            Show(EnemyFrameId.SkulteraTurnLeft5, 8),
            Show(EnemyFrameId.SkulteraTurnLeft6, 10),
            Show(EnemyFrameId.SkulteraTurnLeft7, 13),
            Cue(new(K.FinishTurn)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SkulteraProgram, Animation<SkulteraProgram>> StartingAt { get; } = new Dictionary<SkulteraProgram, Animation<SkulteraProgram>>
    {
        [P.SwimmingLeft] = new([new(P.SwimmingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwimmingLeftLoop] = new([new(P.SwimmingLeftLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TurningRight] = new([new(P.TurningRight, 0, 9, 1)], AnimationEnd.Holds),
        [P.SwimmingRight] = new([new(P.SwimmingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.SwimmingRightLoop] = new([new(P.SwimmingRightLoop, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.TurningLeft] = new([new(P.TurningLeft, 0, 9, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SkulteraProgram> StartingWith(SkulteraProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, SkulteraCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, SkulteraCue> Cue(SkulteraCue cue) => new AnimationBeat<EnemyFrameId, SkulteraCue>.Event(cue);
}
