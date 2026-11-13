using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Hibashi.HibashiCueKind;
using P = SuperMetroid.Core.Enemies.Hibashi.HibashiProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The Hibashi fire pillar's eruption graphics and its collision part as typed programs (#641). Each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="HibashiCue"/> with its typed operands.
/// </summary>
internal static class HibashiAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<HibashiProgram, EnemyFrameId, HibashiCue> Segments { get; } = new(
        new Dictionary<HibashiProgram, AnimationBeat<EnemyFrameId, HibashiCue>[]>
    {
        [P.Graphics] =
        [
            Cue(new(K.PlayEruptionSound)),
            Show(EnemyFrameId.Hibashi00, 2),
            Cue(new(K.ActivityFrame, Value: 0)),
            Show(EnemyFrameId.Hibashi01, 2),
            Cue(new(K.ActivityFrame, Value: 1)),
            Show(EnemyFrameId.Hibashi02, 2),
            Cue(new(K.ActivityFrame, Value: 2)),
            Show(EnemyFrameId.Hibashi03, 2),
            Cue(new(K.ActivityFrame, Value: 3)),
            Show(EnemyFrameId.Hibashi04, 1),
            Cue(new(K.ActivityFrame, Value: 4)),
            Show(EnemyFrameId.Hibashi05, 1),
            Cue(new(K.ActivityFrame, Value: 5)),
            Show(EnemyFrameId.Hibashi06, 1),
            Cue(new(K.ActivityFrame, Value: 6)),
            Show(EnemyFrameId.Hibashi07, 1),
            Cue(new(K.ActivityFrame, Value: 7)),
            Show(EnemyFrameId.Hibashi08, 2),
            Cue(new(K.ActivityFrame, Value: 8)),
            Show(EnemyFrameId.Hibashi09, 2),
            Cue(new(K.ActivityFrame, Value: 9)),
            Show(EnemyFrameId.Hibashi10, 2),
            Cue(new(K.ActivityFrame, Value: 10)),
            Show(EnemyFrameId.Hibashi11, 2),
            Cue(new(K.ActivityFrame, Value: 11)),
            Show(EnemyFrameId.Hibashi12, 2),
            Cue(new(K.ActivityFrame, Value: 12)),
            Show(EnemyFrameId.Hibashi13, 2),
            Cue(new(K.ActivityFrame, Value: 13)),
            Show(EnemyFrameId.Hibashi14, 2),
            Cue(new(K.ActivityFrame, Value: 14)),
            Show(EnemyFrameId.Hibashi15, 2),
            Cue(new(K.ActivityFrame, Value: 15)),
            Show(EnemyFrameId.Hibashi16, 4),
            Cue(new(K.ActivityFrame, Value: 16)),
            Show(EnemyFrameId.Hibashi17, 4),
            Cue(new(K.ActivityFrame, Value: 17)),
            Show(EnemyFrameId.Hibashi18, 4),
            Cue(new(K.ActivityFrame, Value: 18)),
            Show(EnemyFrameId.Hibashi19, 4),
            Cue(new(K.ActivityFrame, Value: 19)),
            Show(EnemyFrameId.Hibashi20, 4),
            Cue(new(K.ActivityFrame, Value: 20)),
            Show(EnemyFrameId.Hibashi21, 4),
            Cue(new(K.ActivityFrame, Value: 21)),
            Show(EnemyFrameId.Hibashi22, 4),
            Cue(new(K.FinishActivity)),
        ],
        [P.Hitbox] =
        [
            Show(EnemyFrameId.Hibashi00, 2),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<HibashiProgram, Animation<HibashiProgram>> StartingAt { get; } = new Dictionary<HibashiProgram, Animation<HibashiProgram>>
    {
        [P.Graphics] = new([new(P.Graphics, 0, 47, 1)], AnimationEnd.Holds),
        [P.Hitbox] = new([new(P.Hitbox, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<HibashiProgram> StartingWith(HibashiProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, HibashiCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, HibashiCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, HibashiCue> Cue(HibashiCue cue) => new AnimationBeat<EnemyFrameId, HibashiCue>.Event(cue);
}
