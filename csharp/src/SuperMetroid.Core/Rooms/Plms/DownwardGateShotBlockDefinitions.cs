using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Compiled mechanics definitions consumed by the downward-gate shot-block setup routine.
/// </summary>
internal static class DownwardGateShotBlockDefinitions
{
    /// <summary>
    /// The eight shot-block triggers in color/side order (blue, red, green, yellow; left then right). Each wakes its
    /// trigger program, and its own side receives a shootable trigger block showing visual blocks $46..$4D in this
    /// order while the opposite side receives nothing.
    /// </summary>
    internal static IReadOnlyList<DownwardGateShotBlockDefinition> Triggers { get; } =
    [
        new(DownwardGateProgram.TriggerBlueLeft, RoomLevelWord.Create(0x46, BlockMirroring.None, RoomCollisionType.ShootableBlock).Raw, 0),
        new(DownwardGateProgram.TriggerBlueRight, 0, RoomLevelWord.Create(0x47, BlockMirroring.None, RoomCollisionType.ShootableBlock).Raw),
        new(DownwardGateProgram.TriggerRedLeft, RoomLevelWord.Create(0x48, BlockMirroring.None, RoomCollisionType.ShootableBlock).Raw, 0),
        new(DownwardGateProgram.TriggerRedRight, 0, RoomLevelWord.Create(0x49, BlockMirroring.None, RoomCollisionType.ShootableBlock).Raw),
        new(DownwardGateProgram.TriggerGreenLeft, RoomLevelWord.Create(0x4a, BlockMirroring.None, RoomCollisionType.ShootableBlock).Raw, 0),
        new(DownwardGateProgram.TriggerGreenRight, 0, RoomLevelWord.Create(0x4b, BlockMirroring.None, RoomCollisionType.ShootableBlock).Raw),
        new(DownwardGateProgram.TriggerYellowLeft, RoomLevelWord.Create(0x4c, BlockMirroring.None, RoomCollisionType.ShootableBlock).Raw, 0),
        new(DownwardGateProgram.TriggerYellowRight, 0, RoomLevelWord.Create(0x4d, BlockMirroring.None, RoomCollisionType.ShootableBlock).Raw),
    ];
}
