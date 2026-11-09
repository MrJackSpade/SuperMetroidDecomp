using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Compiled mechanics definitions consumed by the downward-gate shot-block setup routine.
/// </summary>
internal static class DownwardGateShotBlockDefinitions
{
    /// <summary>
    /// The eight shot-block triggers in color/side order (blue, red, green, yellow; left then right). Each wakes its
    /// trigger's shot-block draw, and its own side receives a shootable trigger block showing blocks 70 to 77 in this
    /// order, whose reaction selects the trigger, while the opposite side receives nothing.
    /// </summary>
    internal static IReadOnlyList<DownwardGateShotBlockDefinition> Triggers { get; } =
    [
        new(DownwardGateTriggerBehavior.BlueLeft, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerBlueLeft)), RoomTerrainBlock.Of(70, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateTriggerBehavior.BlueRight, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerBlueRight)), null, RoomTerrainBlock.Of(71, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
        new(DownwardGateTriggerBehavior.RedLeft, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerRedLeft)), RoomTerrainBlock.Of(72, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateTriggerBehavior.RedRight, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerRedRight)), null, RoomTerrainBlock.Of(73, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
        new(DownwardGateTriggerBehavior.GreenLeft, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerGreenLeft)), RoomTerrainBlock.Of(74, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateTriggerBehavior.GreenRight, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerGreenRight)), null, RoomTerrainBlock.Of(75, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
        new(DownwardGateTriggerBehavior.YellowLeft, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerYellowLeft)), RoomTerrainBlock.Of(76, BlockMirroring.None, RoomCollisionType.ShootableBlock), null),
        new(DownwardGateTriggerBehavior.YellowRight, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerYellowRight)), null, RoomTerrainBlock.Of(77, BlockMirroring.None, RoomCollisionType.ShootableBlock)),
    ];
}
