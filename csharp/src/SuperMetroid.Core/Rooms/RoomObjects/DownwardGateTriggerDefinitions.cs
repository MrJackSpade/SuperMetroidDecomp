namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What each of the eight downward-gate triggers, one per colour and side, draws (#1554): its shot block, once, before its
/// room object leaves the room. Its setup makes the block on the trigger's own side shootable
/// (<see cref="GateObjects.SetupDownwardGateShotBlock"/>).
/// </summary>
internal static class DownwardGateTriggerDefinitions
{
    /// <summary>The change each trigger's room object makes: its shot block drawn once.</summary>
    internal static IReadOnlyDictionary<DownwardGateTriggerBehavior, TerrainChange> Changes { get; } =
        new Dictionary<DownwardGateTriggerBehavior, TerrainChange>
        {
            [DownwardGateTriggerBehavior.BlueLeft] = TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerBlueLeft)),
            [DownwardGateTriggerBehavior.BlueRight] = TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerBlueRight)),
            [DownwardGateTriggerBehavior.RedLeft] = TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerRedLeft)),
            [DownwardGateTriggerBehavior.RedRight] = TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerRedRight)),
            [DownwardGateTriggerBehavior.GreenLeft] = TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerGreenLeft)),
            [DownwardGateTriggerBehavior.GreenRight] = TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerGreenRight)),
            [DownwardGateTriggerBehavior.YellowLeft] = TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerYellowLeft)),
            [DownwardGateTriggerBehavior.YellowRight] = TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerYellowRight)),
        };
}
