using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Rooms.Doors.EyeDoorSweatProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The eye-door sweat drop's falling and floor-splash programs (#1296), in place of their bank-$86 lists ($86:B615..B62C). Each frame shows a <see cref="EyeDoorSweatFrame"/> pose and each
/// command becomes an <see cref="EnemyProjectileCommonCue"/>.
/// </summary>
internal static class EyeDoorSweatAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<EyeDoorSweatProgram, AnimationProgramStep<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue>[]>
    {
        // $86:B615
        [P.Falling] =
        [
            Show(0, 6),
            AnimationProgramStep<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue>.GoTo(P.Falling),
        ],
        // $86:B61D
        [P.HitFloor] =
        [
            Cue(new(CK.ClearPreInstruction)),
            Show(1, 6),
            Show(2, 6),
            Show(3, 6),
            Cue(new(CK.Delete)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => AnimationProgramStep<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue>.Publish(cue);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue> Hold => AnimationProgramStep<EyeDoorSweatProgram, EyeDoorSweatFrame, EnemyProjectileCommonCue>.Hold;
}
