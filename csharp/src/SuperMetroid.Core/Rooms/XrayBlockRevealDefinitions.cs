using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Where X-ray never reveals the room's blocks (#2485): the rooms it excludes and the bosses whose background it keeps.</summary>
internal static class XrayBlockRevealDefinitions
{
    /// <summary>The two rooms X-ray excludes; every other room is unaffected.</summary>
    internal static IReadOnlySet<RoomDefinition> ExcludedRooms { get; } =
        new HashSet<RoomDefinition> { RoomDefinitions.Statues, RoomDefinitions.GlassTunnel };

    /// <summary>
    /// The bosses that keep the room background under X-ray while loaded; every other boss, and no boss, reveals the
    /// blocks.
    /// </summary>
    internal static IReadOnlySet<AreaBoss> BossesPreservingBackground { get; } =
        new HashSet<AreaBoss> { AreaBoss.Kraid, AreaBoss.Crocomire, AreaBoss.Phantoon, AreaBoss.Draygon, AreaBoss.MotherBrain };
}
