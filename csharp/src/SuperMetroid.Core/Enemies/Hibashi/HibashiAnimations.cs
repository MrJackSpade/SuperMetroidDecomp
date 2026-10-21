using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Hibashi.HibashiCueKind;
using P = SuperMetroid.Core.Enemies.Hibashi.HibashiProgram;
using SuperMetroid.Core.Enemies.Hibashi.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>
/// The Hibashi fire pillar's eruption graphics and its collision part as typed programs (#641), in place of their bank-$A6 lists. Generated from the verified native lists: each
/// frame shows a <see cref="HibashiFrame"/> pose and each callback becomes a <see cref="HibashiCue"/> with its typed operands.
/// </summary>
internal static class HibashiAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<HibashiProgram, HibashiFrame, HibashiCue> Programs { get; } = new(
        new Dictionary<HibashiProgram, AnimationProgramStep<HibashiProgram, HibashiFrame, HibashiCue>[]>
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
            Hold,
        ],
        [P.Hitbox] =
        [
            Show(0, 2),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<HibashiProgram, HibashiFrame, HibashiCue> Show(int pose, int duration) => AnimationProgramStep<HibashiProgram, HibashiFrame, HibashiCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<HibashiProgram, HibashiFrame, HibashiCue> Cue(HibashiCue cue) => AnimationProgramStep<HibashiProgram, HibashiFrame, HibashiCue>.Publish(cue);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<HibashiProgram, HibashiFrame, HibashiCue> Hold => AnimationProgramStep<HibashiProgram, HibashiFrame, HibashiCue>.Hold;

}
