using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The common reactions several collision kinds' dispatchers name: the triggers room objects install on the blocks they
/// own, and the runs of reactions that stand for crumble blocks, door caps, station accesses and gate triggers.
/// </summary>
public static class BlockReactionDefinitions
{
    /// <summary>The trigger a resident room object (a coloured door, an eye door, a glass tube) gives its shootable blocks.</summary>
    public static readonly BlockReaction ResidentObjectTrigger = BlockReaction.Common(68);

    /// <summary>The trigger a collectible gives its block, for touch and for shots.</summary>
    public static readonly BlockReaction CollectibleTrigger = BlockReaction.Common(69);

    /// <summary>The trigger a scroll room object gives its special-air block.</summary>
    public static readonly BlockReaction ScrollTrigger = BlockReaction.Common(70);

    /// <summary>The four blue-door caps a shootable block selects, starting at the left-facing cap.</summary>
    public static readonly BlockReactionRun<ColoredDoorOrientation> BlueDoorCaps = new(64,
        ColoredDoorOrientation.Left, ColoredDoorOrientation.Right, ColoredDoorOrientation.Up, ColoredDoorOrientation.Down);

    /// <summary>
    /// The eight crumble blocks a special block selects, from its first common reaction: the one-block, two-wide, two-tall
    /// and two-by-two blocks that grow back, then the same shapes for good.
    /// </summary>
    public static readonly BlockReactionRun<ContactCrumbleBlock> ContactCrumbles = new(0,
        ContactCrumbleBlock.Respawning1x1, ContactCrumbleBlock.Respawning2x1,
        ContactCrumbleBlock.Respawning1x2, ContactCrumbleBlock.Respawning2x2,
        ContactCrumbleBlock.Permanent1x1, ContactCrumbleBlock.Permanent2x1,
        ContactCrumbleBlock.Permanent1x2, ContactCrumbleBlock.Permanent2x2);

    /// <summary>The seven station accesses a special block selects, starting at the map station's right-side access.</summary>
    public static readonly BlockReactionRun<StationAccessBehavior> StationAccesses = new(71,
        StationAccessBehavior.MapRight, StationAccessBehavior.MapLeft,
        StationAccessBehavior.EnergyRight, StationAccessBehavior.EnergyLeft,
        StationAccessBehavior.MissileRight, StationAccessBehavior.MissileLeft,
        StationAccessBehavior.SaveFloor);

    /// <summary>The eight downward-gate triggers a shootable block selects: blue, red, green and yellow, each left then right.</summary>
    public static readonly BlockReactionRun<DownwardGateTriggerBehavior> DownwardGateTriggers = new(70,
        DownwardGateTriggerBehavior.BlueLeft, DownwardGateTriggerBehavior.BlueRight,
        DownwardGateTriggerBehavior.RedLeft, DownwardGateTriggerBehavior.RedRight,
        DownwardGateTriggerBehavior.GreenLeft, DownwardGateTriggerBehavior.GreenRight,
        DownwardGateTriggerBehavior.YellowLeft, DownwardGateTriggerBehavior.YellowRight);
}
