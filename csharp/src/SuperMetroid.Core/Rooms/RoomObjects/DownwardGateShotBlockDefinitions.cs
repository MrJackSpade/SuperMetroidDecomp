using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The eight downward-gate shot-block triggers, one per colour and side. Each wakes its trigger's shot-block draw, and the
/// block on its own side becomes a shootable block whose reaction selects the trigger, keeping what it shows until the draw
/// covers it, while the opposite side is left alone.
/// </summary>
internal static class DownwardGateShotBlockDefinitions
{
    /// <summary>The blue trigger left of its gate.</summary>
    internal static DownwardGateShotBlockDefinition BlueLeft { get; } =
        new(DownwardGateTriggerBehavior.BlueLeft, TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerBlueLeft)), RoomCollisionType.ShootableBlock, null);

    /// <summary>The blue trigger right of its gate.</summary>
    internal static DownwardGateShotBlockDefinition BlueRight { get; } =
        new(DownwardGateTriggerBehavior.BlueRight, TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerBlueRight)), null, RoomCollisionType.ShootableBlock);

    /// <summary>The green trigger left of its gate.</summary>
    internal static DownwardGateShotBlockDefinition GreenLeft { get; } =
        new(DownwardGateTriggerBehavior.GreenLeft, TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerGreenLeft)), RoomCollisionType.ShootableBlock, null);

    /// <summary>The green trigger right of its gate.</summary>
    internal static DownwardGateShotBlockDefinition GreenRight { get; } =
        new(DownwardGateTriggerBehavior.GreenRight, TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerGreenRight)), null, RoomCollisionType.ShootableBlock);
}
