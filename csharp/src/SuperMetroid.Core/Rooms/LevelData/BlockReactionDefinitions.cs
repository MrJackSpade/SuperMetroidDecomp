using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The reactions several collision kinds' responses name that stand for one meaning each: the crumble blocks, door caps,
/// station accesses and gate triggers.
/// </summary>
public static class BlockReactionDefinitions
{
    /// <summary>The four blue-door caps a shootable block selects.</summary>
    public static readonly BlockReactionMeanings<ShootableReaction, ColoredDoorOrientation> BlueDoorCaps = new(
        new Dictionary<ShootableReaction, ColoredDoorOrientation>
        {
            [ShootableReaction.BlueDoorCapLeft] = ColoredDoorOrientation.Left,
            [ShootableReaction.BlueDoorCapRight] = ColoredDoorOrientation.Right,
            [ShootableReaction.BlueDoorCapUp] = ColoredDoorOrientation.Up,
            [ShootableReaction.BlueDoorCapDown] = ColoredDoorOrientation.Down,
        });

    /// <summary>
    /// The eight crumble blocks a special block selects: the one-block, two-wide, two-tall and two-by-two blocks that grow
    /// back, then the same shapes for good.
    /// </summary>
    public static readonly BlockReactionMeanings<SpecialBlockReaction, ContactCrumbleBlock> ContactCrumbles = new(
        new Dictionary<SpecialBlockReaction, ContactCrumbleBlock>
        {
            [SpecialBlockReaction.RespawningCrumble1x1] = ContactCrumbleBlock.Respawning1x1,
            [SpecialBlockReaction.RespawningCrumble2x1] = ContactCrumbleBlock.Respawning2x1,
            [SpecialBlockReaction.RespawningCrumble1x2] = ContactCrumbleBlock.Respawning1x2,
            [SpecialBlockReaction.RespawningCrumble2x2] = ContactCrumbleBlock.Respawning2x2,
            [SpecialBlockReaction.PermanentCrumble1x1] = ContactCrumbleBlock.Permanent1x1,
            [SpecialBlockReaction.PermanentCrumble2x1] = ContactCrumbleBlock.Permanent2x1,
            [SpecialBlockReaction.PermanentCrumble1x2] = ContactCrumbleBlock.Permanent1x2,
            [SpecialBlockReaction.PermanentCrumble2x2] = ContactCrumbleBlock.Permanent2x2,
        });

    /// <summary>
    /// The six station accesses a special block selects, one per station and side. The save station's floor is a reaction
    /// of its own (<see cref="SpecialBlockReaction.SaveStationFloor"/>).
    /// </summary>
    public static readonly BlockReactionMeanings<SpecialBlockReaction, StationAccess> StationAccesses = new(
        new Dictionary<SpecialBlockReaction, StationAccess>
        {
            [SpecialBlockReaction.MapStationRight] = new(StationKind.Map, HorizontalDirection.Right),
            [SpecialBlockReaction.MapStationLeft] = new(StationKind.Map, HorizontalDirection.Left),
            [SpecialBlockReaction.EnergyStationRight] = new(StationKind.Energy, HorizontalDirection.Right),
            [SpecialBlockReaction.EnergyStationLeft] = new(StationKind.Energy, HorizontalDirection.Left),
            [SpecialBlockReaction.MissileStationRight] = new(StationKind.Missile, HorizontalDirection.Right),
            [SpecialBlockReaction.MissileStationLeft] = new(StationKind.Missile, HorizontalDirection.Left),
        });

    /// <summary>The eight downward-gate triggers a shootable block selects: blue, red, green and yellow, each left then right.</summary>
    public static readonly BlockReactionMeanings<ShootableReaction, DownwardGateTrigger> DownwardGateTriggers = new(
        new Dictionary<ShootableReaction, DownwardGateTrigger>
        {
            [ShootableReaction.GateTriggerBlueLeft] = new(DownwardGateTriggerColor.Blue, HorizontalDirection.Left),
            [ShootableReaction.GateTriggerBlueRight] = new(DownwardGateTriggerColor.Blue, HorizontalDirection.Right),
            [ShootableReaction.GateTriggerRedLeft] = new(DownwardGateTriggerColor.Red, HorizontalDirection.Left),
            [ShootableReaction.GateTriggerRedRight] = new(DownwardGateTriggerColor.Red, HorizontalDirection.Right),
            [ShootableReaction.GateTriggerGreenLeft] = new(DownwardGateTriggerColor.Green, HorizontalDirection.Left),
            [ShootableReaction.GateTriggerGreenRight] = new(DownwardGateTriggerColor.Green, HorizontalDirection.Right),
            [ShootableReaction.GateTriggerYellowLeft] = new(DownwardGateTriggerColor.Yellow, HorizontalDirection.Left),
            [ShootableReaction.GateTriggerYellowRight] = new(DownwardGateTriggerColor.Yellow, HorizontalDirection.Right),
        });
}
