using SuperMetroid.Core.Game;

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

    internal static string VisualId(ushort pointer)
    {
        if (Enum.IsDefined((BombBlockRestoreDraw)pointer))
        {
            return (BombBlockRestoreDraw)pointer switch
            {
                BombBlockRestoreDraw.Horizontal => "bomb-horizontal",
                BombBlockRestoreDraw.Vertical => "bomb-vertical",
                BombBlockRestoreDraw.Square => "bomb-square",
                _ => throw new InvalidOperationException($"Undefined {nameof(BombBlockRestoreDraw)} {pointer:X4}."),
            };
        }
        return ClosedNativeWords.Decode<ContactCrumbleRestoreDraw>(pointer, "linked restore draw with a visual ID") switch
        {
            ContactCrumbleRestoreDraw.Horizontal => "crumble-horizontal",
            ContactCrumbleRestoreDraw.Vertical => "crumble-vertical",
            ContactCrumbleRestoreDraw.Square => "crumble-square",
            _ => throw new InvalidOperationException($"Undefined {nameof(ContactCrumbleRestoreDraw)} {pointer:X4}."),
        };
    }

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
