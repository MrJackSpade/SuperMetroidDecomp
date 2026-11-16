using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Cartridge values and helpers of <see cref="DownwardGateShotBlockDefinitions"/> that only verification reads.</summary>
internal static class DownwardGateShotBlockDefinitionsConstants
{
    /// <summary>The red trigger left of its gate.</summary>
    internal static DownwardGateShotBlockDefinition RedLeft { get; } =
        new(DownwardGateTriggerBehavior.RedLeft, TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerRedLeft)), RoomCollisionType.ShootableBlock, null);
    /// <summary>The red trigger right of its gate.</summary>
    internal static DownwardGateShotBlockDefinition RedRight { get; } =
        new(DownwardGateTriggerBehavior.RedRight, TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerRedRight)), null, RoomCollisionType.ShootableBlock);
    /// <summary>The yellow trigger left of its gate.</summary>
    internal static DownwardGateShotBlockDefinition YellowLeft { get; } =
        new(DownwardGateTriggerBehavior.YellowLeft, TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerYellowLeft)), RoomCollisionType.ShootableBlock, null);
    /// <summary>The yellow trigger right of its gate.</summary>
    internal static DownwardGateShotBlockDefinition YellowRight { get; } =
        new(DownwardGateTriggerBehavior.YellowRight, TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerYellowRight)), null, RoomCollisionType.ShootableBlock);
}
