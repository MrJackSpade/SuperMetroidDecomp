namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The six compiled linked-block restoration layouts. Bomb and Samus-contact
/// crumble blocks share a presentation resource, while their distinct
/// collision classes and restoration programs remain in their native catalogs.
/// </summary>
internal static class RoomPlmLinkedRestoreDrawDefinitions
{
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        RoomPlmBombBlockRestoreDrawDefinitions.All.Concat(
            RoomPlmContactCrumbleRestoreDrawDefinitions.All);

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmBombBlockRestoreDrawDefinitions.TryGet(pointer, out list) ||
        RoomPlmContactCrumbleRestoreDrawDefinitions.TryGet(pointer, out list);

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
