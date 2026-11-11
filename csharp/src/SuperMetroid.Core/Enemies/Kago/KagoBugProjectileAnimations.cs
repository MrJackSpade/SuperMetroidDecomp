using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Kago.KagoBugProjectileCueKind;
using P = SuperMetroid.Core.Enemies.Kago.KagoBugProjectileProgram;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>
/// The Kago bugs' landing, falling, jumping and shot programs (#641). Each
/// frame shows a <see cref="KagoBugProjectileFrame"/> pose and each command becomes a <see cref="KagoBugProjectileCue"/> with its typed operands.
/// </summary>
internal static class KagoBugProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KagoBugProjectileProgram, KagoBugProjectileFrame, KagoBugProjectileCue> Segments { get; } = new(
        new Dictionary<KagoBugProjectileProgram, AnimationBeat<KagoBugProjectileFrame, KagoBugProjectileCue>[]>
    {
        [P.SharedDelete] =
        [
            Cue(new(K.Delete)),
        ],
        [P.Landed] =
        [
            Show(0, 5),
            Cue(new(K.StartIdle)),
            Show(1, 32767),
        ],
        [P.Falling] =
        [
            Show(2, 32767),
        ],
        [P.JumpStart] =
        [
            Show(3, 16),
            Show(4, 5),
            Cue(new(K.StartJump)),
        ],
        [P.JumpLoop] =
        [
            Show(5, 32767),
        ],
        [P.Shot] =
        [
            Cue(new(K.DrawFromSharedSheet)),
            Show(6, 4),
            Show(7, 4),
            Show(8, 4),
            Show(9, 4),
            Show(10, 4),
            Cue(new(K.SpawnDrop)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KagoBugProjectileProgram, Animation<KagoBugProjectileProgram>> StartingAt { get; } = new Dictionary<KagoBugProjectileProgram, Animation<KagoBugProjectileProgram>>
    {
        [P.SharedDelete] = new([new(P.SharedDelete, 0, 1, 1)], AnimationEnd.Holds),
        [P.Landed] = new([new(P.Landed, 0, 3, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Falling] = new([new(P.Falling, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.JumpStart] = new([new(P.JumpStart, 0, 3, 1), new(P.JumpLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(1)),
        [P.JumpLoop] = new([new(P.JumpLoop, 0, 1, 1)], AnimationEnd.LoopsFrom(0)),
        [P.Shot] = new([new(P.Shot, 0, 7, 1), new(P.SharedDelete, 0, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KagoBugProjectileProgram> StartingWith(KagoBugProjectileProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<KagoBugProjectileFrame, KagoBugProjectileCue> Show(int pose, int duration) => new AnimationBeat<KagoBugProjectileFrame, KagoBugProjectileCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<KagoBugProjectileFrame, KagoBugProjectileCue> Cue(KagoBugProjectileCue cue) => new AnimationBeat<KagoBugProjectileFrame, KagoBugProjectileCue>.Event(cue);
}
