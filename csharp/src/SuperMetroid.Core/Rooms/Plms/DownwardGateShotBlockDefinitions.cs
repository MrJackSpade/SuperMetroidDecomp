using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Compiled mechanics definitions consumed by the downward-gate shot-block setup routine.
/// </summary>
internal static class DownwardGateShotBlockDefinitions
{
    /// <summary>
    /// The eight shot-block triggers in color/side order (blue, red, green, yellow; left then right). Each wakes its
    /// trigger program, and its own side receives a shootable trigger block showing blocks $46..$4D in this
    /// order while the opposite side receives nothing.
    /// </summary>
    internal static IReadOnlyList<DownwardGateShotBlockDefinition> Triggers { get; } =
    [
        new(DownwardGateProgram.TriggerBlueLeft, RoomTerrainBlock.Of(0x46, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateProgram.TriggerBlueRight, null, RoomTerrainBlock.Of(0x47, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
        new(DownwardGateProgram.TriggerRedLeft, RoomTerrainBlock.Of(0x48, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateProgram.TriggerRedRight, null, RoomTerrainBlock.Of(0x49, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
        new(DownwardGateProgram.TriggerGreenLeft, RoomTerrainBlock.Of(0x4a, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateProgram.TriggerGreenRight, null, RoomTerrainBlock.Of(0x4b, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
        new(DownwardGateProgram.TriggerYellowLeft, RoomTerrainBlock.Of(0x4c, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateProgram.TriggerYellowRight, null, RoomTerrainBlock.Of(0x4d, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
    ];
}
