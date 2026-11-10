using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Hibashi.HibashiCueKind;
using P = SuperMetroid.Core.Enemies.Hibashi.HibashiProgram;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The Hibashi fire pillar's eruption graphics and its collision part as typed programs (#641). Each
/// frame shows a <see cref="HibashiFrame"/> pose and each callback becomes a <see cref="HibashiCue"/> with its typed operands.
/// </summary>
internal static class HibashiAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<HibashiProgram, HibashiFrame, HibashiCue> Segments { get; } = new(
        new Dictionary<HibashiProgram, AnimationBeat<HibashiFrame, HibashiCue>[]>
    {
        [P.Graphics] =
        [
            Cue(new(K.PlayEruptionSound)),
            Show(0, 2),
            Cue(new(K.ActivityFrame, Value: 0)),
            Show(1, 2),
            Cue(new(K.ActivityFrame, Value: 1)),
            Show(2, 2),
            Cue(new(K.ActivityFrame, Value: 2)),
            Show(3, 2),
            Cue(new(K.ActivityFrame, Value: 3)),
            Show(4, 1),
            Cue(new(K.ActivityFrame, Value: 4)),
            Show(5, 1),
            Cue(new(K.ActivityFrame, Value: 5)),
            Show(6, 1),
            Cue(new(K.ActivityFrame, Value: 6)),
            Show(7, 1),
            Cue(new(K.ActivityFrame, Value: 7)),
            Show(8, 2),
            Cue(new(K.ActivityFrame, Value: 8)),
            Show(9, 2),
            Cue(new(K.ActivityFrame, Value: 9)),
            Show(10, 2),
            Cue(new(K.ActivityFrame, Value: 10)),
            Show(11, 2),
            Cue(new(K.ActivityFrame, Value: 11)),
            Show(12, 2),
            Cue(new(K.ActivityFrame, Value: 12)),
            Show(13, 2),
            Cue(new(K.ActivityFrame, Value: 13)),
            Show(14, 2),
            Cue(new(K.ActivityFrame, Value: 14)),
            Show(15, 2),
            Cue(new(K.ActivityFrame, Value: 15)),
            Show(16, 4),
            Cue(new(K.ActivityFrame, Value: 16)),
            Show(17, 4),
            Cue(new(K.ActivityFrame, Value: 17)),
            Show(18, 4),
            Cue(new(K.ActivityFrame, Value: 18)),
            Show(19, 4),
            Cue(new(K.ActivityFrame, Value: 19)),
            Show(20, 4),
            Cue(new(K.ActivityFrame, Value: 20)),
            Show(21, 4),
            Cue(new(K.ActivityFrame, Value: 21)),
            Show(22, 4),
            Cue(new(K.FinishActivity)),
        ],
        [P.Hitbox] =
        [
            Show(0, 2),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<HibashiFrame, HibashiCue> Show(int pose, int duration) => new AnimationBeat<HibashiFrame, HibashiCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<HibashiFrame, HibashiCue> Cue(HibashiCue cue) => new AnimationBeat<HibashiFrame, HibashiCue>.Event(cue);
}
