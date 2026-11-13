using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable bomb and contact-crumble restore visuals, and the editable identities that name them.</summary>
public static class RoomObjectLinkedRestoreVisualFiles
{
    /// <summary>Family-relative JSON filename for the six bomb/contact-crumble horizontal, vertical, and square restoration layouts.</summary>
    public const string VisualFileName = "linked-restores.json";

    /// <summary>The linked restoration as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Linked restore", All, VisualId);

    /// <summary>Pictures every linked restoration frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed linked restoration pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectLinkedRestoreVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The three bomb-block and three contact-crumble restoration layouts, bomb blocks first.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All =>
        LinkedRestoreObjectFamily.Families.SelectMany(AllOf);

    /// <summary>The horizontal, vertical and square restoration layouts of <paramref name="family"/>, in that order.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> AllOf(LinkedRestoreObjectFamily family)
    {
        foreach (RoomObjectDraw candidate in RestorationDraws())
        {
            if (family.TryDescribe(candidate, out LinkedRestoreObjectFamily.Draw draw))
                yield return Export(candidate, draw);
        }
    }

    /// <summary>Builds the draw list of the restoration <paramref name="request"/>, whose shape is <paramref name="draw"/>.</summary>
    internal static RoomObjectShotBlockDrawDefinitions.DrawList Export(RoomObjectDraw request, LinkedRestoreObjectFamily.Draw draw) =>
        RoomObjectDrawListExport.Runs(request, draw.RunCount, static _ => 2, draw.CollisionAt,

            _ => draw.Vertical ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal,
            RoomObjectDrawListExport.Stacked);

    /// <summary>Maps a restoration draw to its bomb or crumble artwork key and rejects other draws.</summary>
    internal static string VisualId(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Horizontal } => "bomb-horizontal",
        RoomObjectFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Vertical } => "bomb-vertical",
        RoomObjectFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Square } => "bomb-square",
        RoomObjectFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Horizontal } => "crumble-horizontal",
        RoomObjectFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Vertical } => "crumble-vertical",
        RoomObjectFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Square } => "crumble-square",
        _ => throw new InvalidDataException($"{draw} is not a linked restoration draw."),
    };

    /// <summary>Every bomb-block and contact-crumble restoration draw, each family's in horizontal, vertical, square order.</summary>
    private static IEnumerable<RoomObjectDraw> RestorationDraws() =>
        Enum.GetValues<BombBlockRestoreDraw>().Select(draw => (RoomObjectDraw)RoomObjectDraw.Of(draw))
            .Concat(Enum.GetValues<ContactCrumbleRestoreDraw>().Select(draw => (RoomObjectDraw)RoomObjectDraw.Of(draw)));
}
