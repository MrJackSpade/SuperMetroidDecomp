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

    /// <summary>Names a linked restoration by its owning family and decoded layout.</summary>
    internal static string VisualId(ushort pointer)
    {
        if (RoomPlmBombBlockRestoreDrawDefinitions.TryDescribe(pointer, out var bomb))
            return "bomb-" + LayoutId(bomb.Vertical, bomb.Square);
        if (RoomPlmContactCrumbleRestoreDrawDefinitions.TryDescribe(pointer, out var crumble))
            return "crumble-" + LayoutId(crumble.Vertical, crumble.Square);
        throw new InvalidDataException($"Linked restore draw ${pointer:X4} has no visual ID.");
    }

    private static string LayoutId(bool vertical, bool square) =>
        square ? "square" : vertical ? "vertical" : "horizontal";

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
