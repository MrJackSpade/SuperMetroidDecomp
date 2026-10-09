namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The six compiled linked-block restoration layouts. Bomb and Samus-contact
/// crumble blocks share a presentation resource, while their distinct
/// collision classes and restoration programs remain in their native catalogs.
/// </summary>
internal static class RoomPlmLinkedRestoreDrawDefinitions
{
    /// <summary>Combined draw lists for bomb-restoring blocks and Samus-contact crumble blocks.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        RoomPlmBombBlockRestoreDrawDefinitions.All.Concat(
            RoomPlmContactCrumbleRestoreDrawDefinitions.All);

    /// <summary>Maps a linked-restore draw pointer to the stable visual identifier used by extracted assets.</summary>
    /// <param name="pointer">Instruction-list pointer for one of the supported bomb or crumble block layouts.</param>
    /// <returns>The matching visual identifier.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a linked-restore draw list.</exception>
    internal static string VisualId(ushort pointer) => pointer switch
    {
        RoomPlmBombBlockRestoreDrawDefinitions.Horizontal => "bomb-horizontal",
        RoomPlmBombBlockRestoreDrawDefinitions.Vertical => "bomb-vertical",
        RoomPlmBombBlockRestoreDrawDefinitions.Square => "bomb-square",
        RoomPlmContactCrumbleRestoreDrawDefinitions.Horizontal => "crumble-horizontal",
        RoomPlmContactCrumbleRestoreDrawDefinitions.Vertical => "crumble-vertical",
        RoomPlmContactCrumbleRestoreDrawDefinitions.Square => "crumble-square",
        _ => throw new InvalidDataException(
            $"Linked restore draw ${pointer:X4} has no visual ID."),
    };

    /// <summary>Finds a linked-restore draw list by its case-sensitive visual identifier.</summary>
    /// <param name="id">Identifier previously associated with a linked-restore draw list.</param>
    /// <param name="list">Receives the matching draw list when found; otherwise receives its default value.</param>
    /// <returns><see langword="true"/> when an identifier matches; otherwise, <see langword="false"/>.</returns>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in All)
        {
            if (string.Equals(id, VisualId(candidate.Pointer), StringComparison.Ordinal))
            {
                list = candidate;
                return true;
            }
        }
        list = default;
        return false;
    }
}
