using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.RoomPlmLinkedRestoreDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The artwork names of the linked-restore draws.</summary>
internal static class LinkedRestorePlmDraws
{
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
        ShotBlockPlmDraws.FindByVisualId(All, VisualId, id, out list);
}
