
namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The six compiled linked-block restoration layouts. Bomb and Samus-contact
/// crumble blocks share a presentation resource, while their distinct
/// collision classes and restoration programs remain in their native catalogs.
/// </summary>
internal static class RoomPlmLinkedRestoreDrawDefinitions
{
    /// <summary>Combines the three bomb-block and three contact-crumble restoration layouts for shared visual export.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        LinkedRestorePlmFamily.Families.SelectMany(family => family.All);
}
