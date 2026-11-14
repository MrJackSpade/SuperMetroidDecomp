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

    /// <summary>The seven station accesses a special block selects.</summary>
    public static readonly BlockReactionMeanings<SpecialBlockReaction, StationAccessBehavior> StationAccesses = new(
        new Dictionary<SpecialBlockReaction, StationAccessBehavior>
        {
            [SpecialBlockReaction.MapStationRight] = StationAccessBehavior.MapRight,
            [SpecialBlockReaction.MapStationLeft] = StationAccessBehavior.MapLeft,
            [SpecialBlockReaction.EnergyStationRight] = StationAccessBehavior.EnergyRight,
            [SpecialBlockReaction.EnergyStationLeft] = StationAccessBehavior.EnergyLeft,
            [SpecialBlockReaction.MissileStationRight] = StationAccessBehavior.MissileRight,
            [SpecialBlockReaction.MissileStationLeft] = StationAccessBehavior.MissileLeft,
            [SpecialBlockReaction.SaveStationFloor] = StationAccessBehavior.SaveFloor,
        });

    /// <summary>The eight downward-gate triggers a shootable block selects: blue, red, green and yellow, each left then right.</summary>
    public static readonly BlockReactionMeanings<ShootableReaction, DownwardGateTriggerBehavior> DownwardGateTriggers = new(
        new Dictionary<ShootableReaction, DownwardGateTriggerBehavior>
        {
            [ShootableReaction.GateTriggerBlueLeft] = DownwardGateTriggerBehavior.BlueLeft,
            [ShootableReaction.GateTriggerBlueRight] = DownwardGateTriggerBehavior.BlueRight,
            [ShootableReaction.GateTriggerRedLeft] = DownwardGateTriggerBehavior.RedLeft,
            [ShootableReaction.GateTriggerRedRight] = DownwardGateTriggerBehavior.RedRight,
            [ShootableReaction.GateTriggerGreenLeft] = DownwardGateTriggerBehavior.GreenLeft,
            [ShootableReaction.GateTriggerGreenRight] = DownwardGateTriggerBehavior.GreenRight,
            [ShootableReaction.GateTriggerYellowLeft] = DownwardGateTriggerBehavior.YellowLeft,
            [ShootableReaction.GateTriggerYellowRight] = DownwardGateTriggerBehavior.YellowRight,
        });
}
