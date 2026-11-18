using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The eight downward-gate triggers, one per colour and side (#1554, #2484): what each draws, its shot block once before
/// its room object leaves the room, and which shots set it off. A green trigger takes only Super Missiles, a red one
/// Missiles or Super Missiles, a blue one anything but a Power Bomb; the left yellow trigger takes only a Power Bomb, and
/// the right one, the opposite, anything but. Only a blue trigger turns a shot away without a sound. Its setup makes the
/// block on the trigger's own side shootable (<see cref="GateObjects.SetupDownwardGateShotBlock"/>).
/// </summary>
internal static class DownwardGateTriggerDefinitions
{
    /// <summary>Each trigger's definition.</summary>
    internal static IReadOnlyDictionary<DownwardGateTrigger, DownwardGateTriggerDefinition> Triggers { get; } =
        new Dictionary<DownwardGateTrigger, DownwardGateTriggerDefinition>
        {
            [new(DownwardGateTriggerColor.Blue, HorizontalDirection.Left)] = new(
                TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerBlueLeft)),
                DownwardGateTriggerShots.AllBut(SamusProjectileFamily.PowerBomb), RejectionSound: null),
            [new(DownwardGateTriggerColor.Blue, HorizontalDirection.Right)] = new(
                TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerBlueRight)),
                DownwardGateTriggerShots.AllBut(SamusProjectileFamily.PowerBomb), RejectionSound: null),
            [new(DownwardGateTriggerColor.Red, HorizontalDirection.Left)] = new(
                TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerRedLeft)),
                DownwardGateTriggerShots.Only(SamusProjectileFamily.Missile, SamusProjectileFamily.SuperMissile), DownwardGateDefinitions.RejectedShotSound),
            [new(DownwardGateTriggerColor.Red, HorizontalDirection.Right)] = new(
                TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerRedRight)),
                DownwardGateTriggerShots.Only(SamusProjectileFamily.Missile, SamusProjectileFamily.SuperMissile), DownwardGateDefinitions.RejectedShotSound),
            [new(DownwardGateTriggerColor.Green, HorizontalDirection.Left)] = new(
                TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerGreenLeft)),
                DownwardGateTriggerShots.Only(SamusProjectileFamily.SuperMissile), DownwardGateDefinitions.RejectedShotSound),
            [new(DownwardGateTriggerColor.Green, HorizontalDirection.Right)] = new(
                TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerGreenRight)),
                DownwardGateTriggerShots.Only(SamusProjectileFamily.SuperMissile), DownwardGateDefinitions.RejectedShotSound),
            [new(DownwardGateTriggerColor.Yellow, HorizontalDirection.Left)] = new(
                TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerYellowLeft)),
                DownwardGateTriggerShots.Only(SamusProjectileFamily.PowerBomb), DownwardGateDefinitions.RejectedShotSound),
            [new(DownwardGateTriggerColor.Yellow, HorizontalDirection.Right)] = new(
                TerrainChange.DrawnOnce(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.TriggerYellowRight)),
                DownwardGateTriggerShots.AllBut(SamusProjectileFamily.PowerBomb), DownwardGateDefinitions.RejectedShotSound),
        };
}
