using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of Crocomire's five bridge and invisible-wall frames (#1557), and the published names that key them.</summary>
public static class RoomObjectCrocomireVisualFiles
{
    /// <summary>JSON filename for Crocomire bridge and invisible-wall visual layouts.</summary>
    public const string VisualFileName = "crocomire-arena.json";

    /// <summary>The Crocomire arena as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Crocomire arena", All, VisualId);

    /// <summary>Pictures every Crocomire arena frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Crocomire arena pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectCrocomireVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The draw lists of every bridge and wall draw.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (CrocomireArenaDraw frame in Enum.GetValues<CrocomireArenaDraw>())
            {
                TryGet(RoomObjectDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>
    /// The draw list of one bridge or wall frame as the cartridge writes it: the runs of its draw list, each cell
    /// colliding as the game's block shape for the frame says (#1608). The run form is the cartridge's; the game draws
    /// the shape.
    /// </summary>
    internal static bool TryGet(RoomObjectDraw draw, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        if (!CrocomireArenaDraws.TryDescribe(draw, out RoomBlockShape? described))
        {
            list = default;
            return false;
        }
        RoomBlockShape shape = described;
        // A wall is written as its three columns, each down from the wall's top row and started from the origin rather
        // than from the column before. A bridge is written as the one run along its row.
        Offset16<RoomBlocks> belowOrigin = new(0, 1);
        Offset16<RoomBlocks>[] wallStarts = [new(0, 0), new(1, 0), new(2, 0)];
        int wallColumnHeight = 8;
        if (shape.Covers(belowOrigin))
        {
            list = RoomObjectDrawListExport.Runs(draw, wallStarts.Length, _ => wallColumnHeight,
                (run, cell) => shape.CollisionAt(wallStarts[run] + new Offset16<RoomBlocks>(0, cell)),
                _ => RoomObjectRunDirection.Vertical, run => wallStarts[run]);
            return true;
        }
        list = RoomObjectDrawListExport.Runs(draw, 1, _ => shape.Cells.Count,
            (_, cell) => shape.CollisionAt(new Offset16<RoomBlocks>(cell, 0)),
            RoomObjectDrawListExport.Horizontal, _ => Offset16<RoomBlocks>.Zero);
        return true;
    }

    /// <summary>Maps an arena draw to the published bridge or wall artwork key.</summary>
    internal static string VisualId(RoomObjectDraw draw) =>
        RoomObjectVisualIdDefinitions.CrocomireArena[RoomObjectDraw.FrameOf<CrocomireArenaDraw>(draw, "Crocomire arena")];
}
