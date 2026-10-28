namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The seventy distinct PLMs retail room populations place; every other PLM is rejected. Setup and execution stay with
/// the PLM families.
/// </summary>
internal static partial class RoomPlmHeaderDefinitions
{
    /// <summary>
    /// The contact-crumble header of each special-block reaction, in reaction order: the 1x1, 2x1, 1x2 and 2x2
    /// respawning blocks, then the same sizes as permanent blocks.
    /// </summary>
    internal static IReadOnlyList<PlmHeaderId> ContactCrumbleHeaders { get; } =
    [
        PlmHeaderId.ContactCrumble1x1Respawning, PlmHeaderId.ContactCrumble2x1Respawning,
        PlmHeaderId.ContactCrumble1x2Respawning, PlmHeaderId.ContactCrumble2x2Respawning,
        PlmHeaderId.ContactCrumble1x1Permanent, PlmHeaderId.ContactCrumble2x1Permanent,
        PlmHeaderId.ContactCrumble1x2Permanent, PlmHeaderId.ContactCrumble2x2Permanent,
    ];
}
