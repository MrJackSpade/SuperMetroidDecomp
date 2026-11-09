using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Compiled mechanics definitions consumed by the downward-gate shot-block setup routine.
/// </summary>
internal static class DownwardGateShotBlockDefinitions
{
    /// <summary>
    /// The eight shot-block triggers in color/side order (blue, red, green, yellow; left then right). Each wakes its
    /// trigger program, and its own side receives a shootable trigger block showing blocks 70 to 77 in this
    /// order, whose reaction selects the trigger, while the opposite side receives nothing.
    /// </summary>
    internal static IReadOnlyList<DownwardGateShotBlockDefinition> Triggers { get; } =
    [
        new(DownwardGateTriggerBehavior.BlueLeft, DownwardGateProgram.TriggerBlueLeft, RoomTerrainBlock.Of(70, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateTriggerBehavior.BlueRight, DownwardGateProgram.TriggerBlueRight, null, RoomTerrainBlock.Of(71, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
        new(DownwardGateTriggerBehavior.RedLeft, DownwardGateProgram.TriggerRedLeft, RoomTerrainBlock.Of(72, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateTriggerBehavior.RedRight, DownwardGateProgram.TriggerRedRight, null, RoomTerrainBlock.Of(73, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
        new(DownwardGateTriggerBehavior.GreenLeft, DownwardGateProgram.TriggerGreenLeft, RoomTerrainBlock.Of(74, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateTriggerBehavior.GreenRight, DownwardGateProgram.TriggerGreenRight, null, RoomTerrainBlock.Of(75, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
        new(DownwardGateTriggerBehavior.YellowLeft, DownwardGateProgram.TriggerYellowLeft, RoomTerrainBlock.Of(76, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateTriggerBehavior.YellowRight, DownwardGateProgram.TriggerYellowRight, null, RoomTerrainBlock.Of(77, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
    ];
}
