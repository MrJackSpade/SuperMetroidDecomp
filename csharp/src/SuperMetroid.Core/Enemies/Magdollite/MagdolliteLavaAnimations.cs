using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Magdollite.MagdolliteLavaCueKind;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteLavaProgram;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite's thrown-lava programs (#641). Each
/// frame shows a <see cref="MagdolliteLavaFrame"/> pose and each command becomes a <see cref="MagdolliteLavaCue"/> with its typed operands.
/// </summary>
internal static class MagdolliteLavaAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue> Segments { get; } = new(
        new Dictionary<MagdolliteLavaProgram, AnimationBeat<MagdolliteLavaFrame, MagdolliteLavaCue>[]>
    {
        [P.SharedDelete] =
        [
            Cue(new(K.Delete)),
        ],
        [P.Left] =
        [
            Show(0, 1),
        ],
        [P.Right] =
        [
            Show(1, 1),
        ],
        [P.Shot] =
        [
            Cue(new(K.SpawnDrops)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<MagdolliteLavaProgram, Animation<MagdolliteLavaProgram>> StartingAt { get; } = new Dictionary<MagdolliteLavaProgram, Animation<MagdolliteLavaProgram>>
    {
        [P.SharedDelete] = new([new(P.SharedDelete, 0, 1, 1)], AnimationEnd.Holds),
        [P.Left] = new([new(P.Left, 0, 1, 1)], AnimationEnd.Holds),
        [P.Right] = new([new(P.Right, 0, 1, 1)], AnimationEnd.Holds),
        [P.Shot] = new([new(P.Shot, 0, 1, 1), new(P.SharedDelete, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<MagdolliteLavaProgram> StartingWith(MagdolliteLavaProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<MagdolliteLavaFrame, MagdolliteLavaCue> Show(int pose, int duration) => new AnimationBeat<MagdolliteLavaFrame, MagdolliteLavaCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<MagdolliteLavaFrame, MagdolliteLavaCue> Cue(MagdolliteLavaCue cue) => new AnimationBeat<MagdolliteLavaFrame, MagdolliteLavaCue>.Event(cue);
}
