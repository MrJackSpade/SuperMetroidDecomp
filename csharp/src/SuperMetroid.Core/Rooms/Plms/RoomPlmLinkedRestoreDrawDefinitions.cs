using SuperMetroid.Core.Rooms.LevelData;

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
        RoomPlmLinkedRestoreDefinition.Families.SelectMany(family => family.All);

    /// <summary>Maps a restoration draw to its bomb or crumble artwork key and rejects other draws.</summary>
    internal static string VisualId(PlmDraw draw) => draw switch
    {
        PlmFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Horizontal } => "bomb-horizontal",
        PlmFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Vertical } => "bomb-vertical",
        PlmFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Square } => "bomb-square",
        PlmFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Horizontal } => "crumble-horizontal",
        PlmFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Vertical } => "crumble-vertical",
        PlmFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Square } => "crumble-square",
        _ => throw new InvalidDataException($"{draw} is not a linked restoration draw."),
    };

    /// <summary>Finds the compiled restoration draw whose stable visual key equals the supplied identifier.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(All, VisualId, id, out list);
}
