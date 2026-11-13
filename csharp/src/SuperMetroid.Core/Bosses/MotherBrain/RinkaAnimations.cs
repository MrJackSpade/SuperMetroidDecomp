using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Bosses.MotherBrain.RinkaCueKind;
using P = SuperMetroid.Core.Bosses.MotherBrain.RinkaProgram;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The ordinary and Mother Brain Rinkas' hide, fire and spin animations as typed programs (#641), in place of their lists. Generated from the verified lists: each
/// frame shows a <see cref="EnemyFrameId"/> and each callback becomes a <see cref="RinkaCue"/> with its typed operands.
/// </summary>
internal static class RinkaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<RinkaProgram, EnemyFrameId, RinkaCue> Segments { get; } = new(
        new Dictionary<RinkaProgram, AnimationBeat<EnemyFrameId, RinkaCue>[]>
    {
        [P.Ordinary] =
        [
            Cue(new(K.Hide)),
            Show(EnemyFrameId.RinkaSpin0, 64),
            Cue(new(K.Fire)),
        ],
        [P.OrdinaryLoop] =
        [
            Show(EnemyFrameId.RinkaSpin0, 16),
            Show(EnemyFrameId.RinkaSpin1, 8),
            Show(EnemyFrameId.RinkaSpin2, 7),
            Show(EnemyFrameId.RinkaSpin3, 6),
            Show(EnemyFrameId.RinkaSpin4, 5),
            Show(EnemyFrameId.RinkaSpin3, 6),
            Show(EnemyFrameId.RinkaSpin2, 7),
            Show(EnemyFrameId.RinkaSpin1, 8),
        ],
        [P.Special] =
        [
            Cue(new(K.HideAndProcessOffScreen)),
            Show(EnemyFrameId.RinkaSpin0, 64),
            Cue(new(K.Fire)),
        ],
        [P.SpecialLoop] =
        [
            Show(EnemyFrameId.RinkaSpin0, 16),
            Show(EnemyFrameId.RinkaSpin1, 8),
            Show(EnemyFrameId.RinkaSpin2, 7),
            Show(EnemyFrameId.RinkaSpin3, 6),
            Show(EnemyFrameId.RinkaSpin4, 5),
            Show(EnemyFrameId.RinkaSpin3, 6),
            Show(EnemyFrameId.RinkaSpin2, 7),
            Show(EnemyFrameId.RinkaSpin1, 8),
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

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyFrameId, RinkaCue> Show(EnemyFrameId frame, int duration) => new AnimationBeat<EnemyFrameId, RinkaCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyFrameId, RinkaCue> Cue(RinkaCue cue) => new AnimationBeat<EnemyFrameId, RinkaCue>.Event(cue);
}
