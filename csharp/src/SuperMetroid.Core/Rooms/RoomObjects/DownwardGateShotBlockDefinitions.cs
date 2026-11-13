using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Compiled mechanics definitions consumed by the downward-gate shot-block setup.
/// </summary>
internal static class DownwardGateShotBlockDefinitions
{
    /// <summary>
    /// The eight shot-block triggers in color/side order (blue, red, green, yellow; left then right). Each wakes its
    /// trigger's shot-block draw, and the block on its own side becomes a shootable block whose reaction selects the
    /// trigger, keeping what it shows until the draw covers it, while the opposite side is left alone.
    /// </summary>
    internal static IReadOnlyList<DownwardGateShotBlockDefinition> Triggers { get; } =
    [
        new(DownwardGateTriggerBehavior.BlueLeft, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerBlueLeft)), RoomCollisionType.ShootableBlock, null),
        new(DownwardGateTriggerBehavior.BlueRight, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerBlueRight)), null, RoomCollisionType.ShootableBlock),
        new(DownwardGateTriggerBehavior.RedLeft, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerRedLeft)), RoomCollisionType.ShootableBlock, null),
        new(DownwardGateTriggerBehavior.RedRight, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerRedRight)), null, RoomCollisionType.ShootableBlock),
        new(DownwardGateTriggerBehavior.GreenLeft, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerGreenLeft)), RoomCollisionType.ShootableBlock, null),
        new(DownwardGateTriggerBehavior.GreenRight, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerGreenRight)), null, RoomCollisionType.ShootableBlock),
        new(DownwardGateTriggerBehavior.YellowLeft, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerYellowLeft)), RoomCollisionType.ShootableBlock, null),
        new(DownwardGateTriggerBehavior.YellowRight, TerrainChange.DrawnOnce(RoomObjectDraw.Of(DownwardGateDraw.TriggerYellowRight)), null, RoomCollisionType.ShootableBlock),
    ];
}
