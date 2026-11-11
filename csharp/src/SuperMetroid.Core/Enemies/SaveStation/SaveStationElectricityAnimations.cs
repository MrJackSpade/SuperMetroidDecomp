using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Rooms.RoomObjects.SaveStationElectricityProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.SaveStation;

/// <summary>
/// The save-station electricity's twenty eight-pose cycles (#1296). Each frame shows a <see cref="SaveStationElectricityFrame"/> pose and each
/// command becomes an <see cref="EnemyProjectileCommonCue"/>.
/// </summary>
internal static class SaveStationElectricityAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<SaveStationElectricityProgram, SaveStationElectricityFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<SaveStationElectricityProgram, AnimationBeat<SaveStationElectricityFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Start] =
        [
        ],
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
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<SaveStationElectricityProgram, Animation<SaveStationElectricityProgram>> StartingAt { get; } = new Dictionary<SaveStationElectricityProgram, Animation<SaveStationElectricityProgram>>
    {
        [P.Start] = new([new(P.Cycle, 0, 8, 20), new(P.Cycle, 8, 1, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<SaveStationElectricityProgram> StartingWith(SaveStationElectricityProgram segment) => StartingAt[segment];

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<SaveStationElectricityFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<SaveStationElectricityFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<SaveStationElectricityFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<SaveStationElectricityFrame, EnemyProjectileCommonCue>.Event(cue);
}
