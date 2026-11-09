namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The seventy distinct room objects retail room populations place; every other room object is rejected. Setup and execution stay with
/// the room-object families.
/// </summary>
internal static partial class RoomObjectHeaderDefinitions
{
    /// <summary>
    /// The contact-crumble header of each special-block reaction, in reaction order: the 1x1, 2x1, 1x2 and 2x2
    /// respawning blocks, then the same sizes as permanent blocks.
    /// </summary>
    internal static IReadOnlyList<RoomObjectHeaderId> ContactCrumbleHeaders { get; } =
    [
        RoomObjectHeaderId.ContactCrumble1x1Respawning, RoomObjectHeaderId.ContactCrumble2x1Respawning,
        RoomObjectHeaderId.ContactCrumble1x2Respawning, RoomObjectHeaderId.ContactCrumble2x2Respawning,
        RoomObjectHeaderId.ContactCrumble1x1Permanent, RoomObjectHeaderId.ContactCrumble2x1Permanent,
        RoomObjectHeaderId.ContactCrumble1x2Permanent, RoomObjectHeaderId.ContactCrumble2x2Permanent,
    ];
}
