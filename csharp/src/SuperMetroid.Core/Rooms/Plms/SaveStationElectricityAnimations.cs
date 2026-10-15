using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Rooms.Plms.SaveStationElectricityProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The save-station electricity's twenty eight-pose cycles (#1296), in place of their bank-$86 lists ($86:E683..E6AC). Each frame shows a <see cref="SaveStationElectricityFrame"/> pose and each
/// command becomes an <see cref="EnemyProjectileCommonCue"/>.
/// </summary>
internal static class SaveStationElectricityAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<SaveStationElectricityProgram, AnimationProgramStep<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue>[]>
    {
        // $86:E683
        [P.Start] =
        [
            AnimationProgramStep<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue>.SetCounter(20),
            AnimationProgramStep<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue>.FallInto(P.Cycle),
        ],
        // $86:E687
        [P.Cycle] =
        [
            Show(0, 1),
            Show(1, 1),
            Show(2, 1),
            Show(3, 1),
            Show(4, 1),
            Show(5, 1),
            Show(6, 1),
            Show(7, 1),
            AnimationProgramStep<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue>.RepeatWhileCounting(P.Cycle),
            Cue(new(CK.Delete)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => AnimationProgramStep<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue>.Publish(cue);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue> Hold => AnimationProgramStep<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue>.Hold;
}
