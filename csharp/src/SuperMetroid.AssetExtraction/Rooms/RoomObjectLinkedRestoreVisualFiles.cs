using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable bomb and contact-crumble restore visuals, and the editable identities that name them.</summary>
public static class RoomObjectLinkedRestoreVisualFiles
{
    /// <summary>Family-relative JSON filename for the six bomb/contact-crumble horizontal, vertical, and square restoration layouts.</summary>
    private const string VisualFileName = "linked-restores.json";

    /// <summary>The arrangements of each kind's restorations in the order the import publishes them: side by side, stacked, square.</summary>
    private static readonly BlockArrangement[] ExportOrder = [BlockArrangement.Horizontal, BlockArrangement.Vertical, BlockArrangement.Square];

    /// <summary>The bomb blocks' restorations, in the order the import publishes them.</summary>
    private static IReadOnlyList<RoomObjectFrameDraw<BombBlockRestoreDraw>> BombBlockRestorations { get; } =
        [.. ExportOrder.Select(layout => LinkedRestoreObjectFamily.BombBlockDraws.Of(new BombBlockRestoreDraw(layout)))];

    /// <summary>The contact crumble blocks' restorations, in the order the import publishes them.</summary>
    private static IReadOnlyList<RoomObjectFrameDraw<ContactCrumbleRestoreDraw>> ContactCrumbleRestorations { get; } =
        [.. ExportOrder.Select(layout => LinkedRestoreObjectFamily.ContactCrumbleDraws.Of(new ContactCrumbleRestoreDraw(layout)))];

    /// <summary>
    /// The linked restoration as the import pictures them: every frame's geometry and published name. It reads the
    /// restorations as it is built, so they are declared before it.
    /// </summary>
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
    internal static IEnumerable<RoomObjectDrawLists.DrawList> All =>
        BombBlockRestorations.Select(Export).Concat(ContactCrumbleRestorations.Select(Export));

    /// <summary>Builds the draw list of the bomb-block restoration <paramref name="draw"/>.</summary>
    private static RoomObjectDrawLists.DrawList Export(RoomObjectFrameDraw<BombBlockRestoreDraw> draw) =>
        RoomObjectShapeRun.DrawList(Ensure.NotNull(draw), draw.Frame.Blocks, RoomObjectShapeRun.Arrangement(draw.Frame.Layout));

    /// <summary>Builds the draw list of the contact crumble restoration <paramref name="draw"/>.</summary>
    private static RoomObjectDrawLists.DrawList Export(RoomObjectFrameDraw<ContactCrumbleRestoreDraw> draw) =>
        RoomObjectShapeRun.DrawList(Ensure.NotNull(draw), draw.Frame.Blocks, RoomObjectShapeRun.Arrangement(draw.Frame.Layout));

    /// <summary>Maps a restoration draw to its bomb or crumble artwork key and rejects other draws.</summary>
    internal static string VisualId(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<BombBlockRestoreDraw> { Frame.Layout: BlockArrangement.Horizontal } => "bomb-horizontal",
        RoomObjectFrameDraw<BombBlockRestoreDraw> { Frame.Layout: BlockArrangement.Vertical } => "bomb-vertical",
        RoomObjectFrameDraw<BombBlockRestoreDraw> { Frame.Layout: BlockArrangement.Square } => "bomb-square",
        RoomObjectFrameDraw<ContactCrumbleRestoreDraw> { Frame.Layout: BlockArrangement.Horizontal } => "crumble-horizontal",
        RoomObjectFrameDraw<ContactCrumbleRestoreDraw> { Frame.Layout: BlockArrangement.Vertical } => "crumble-vertical",
        RoomObjectFrameDraw<ContactCrumbleRestoreDraw> { Frame.Layout: BlockArrangement.Square } => "crumble-square",
        _ => throw new InvalidDataException($"{draw} is not a linked restoration draw."),
    };

}
