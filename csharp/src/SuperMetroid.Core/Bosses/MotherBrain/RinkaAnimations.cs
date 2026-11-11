using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.MotherBrain.RinkaCueKind;
using P = SuperMetroid.Core.Bosses.MotherBrain.RinkaProgram;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The ordinary and Mother Brain Rinkas' hide, fire and spin animations as typed programs (#641), in place of their lists. Generated from the verified lists: each
/// frame shows a <see cref="RinkaFrame"/> pose and each callback becomes a <see cref="RinkaCue"/> with its typed operands.
/// </summary>
internal static class RinkaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RinkaProgram, RinkaFrame, RinkaCue> Segments { get; } = new(
        new Dictionary<RinkaProgram, AnimationBeat<RinkaFrame, RinkaCue>[]>
    {
        [P.Ordinary] =
        [
            Cue(new(K.Hide)),
            Show(0, 64),
            Cue(new(K.Fire)),
        ],
        [P.OrdinaryLoop] =
        [
            Show(0, 16),
            Show(1, 8),
            Show(2, 7),
            Show(3, 6),
            Show(4, 5),
            Show(3, 6),
            Show(2, 7),
            Show(1, 8),
        ],
        [P.Special] =
        [
            Cue(new(K.HideAndProcessOffScreen)),
            Show(0, 64),
            Cue(new(K.Fire)),
        ],
        [P.SpecialLoop] =
        [
            Show(0, 16),
            Show(1, 8),
            Show(2, 7),
            Show(3, 6),
            Show(4, 5),
            Show(3, 6),
            Show(2, 7),
            Show(1, 8),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<RinkaProgram, Animation<RinkaProgram>> StartingAt { get; } = new Dictionary<RinkaProgram, Animation<RinkaProgram>>
    {
        [P.Ordinary] = new([new(P.Ordinary, 0, 3, 1), new(P.OrdinaryLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.OrdinaryLoop] = new([new(P.OrdinaryLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Special] = new([new(P.Special, 0, 3, 1), new(P.SpecialLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(1)),
        [P.SpecialLoop] = new([new(P.SpecialLoop, 0, 8, 1)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<RinkaProgram> StartingWith(RinkaProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<RinkaFrame, RinkaCue> Show(int pose, int duration) => new AnimationBeat<RinkaFrame, RinkaCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<RinkaFrame, RinkaCue> Cue(RinkaCue cue) => new AnimationBeat<RinkaFrame, RinkaCue>.Event(cue);
}
