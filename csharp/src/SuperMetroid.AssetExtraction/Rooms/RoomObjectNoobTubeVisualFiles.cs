using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge-checked, strictly visual-only installed n00b-tube artwork.</summary>
public static class RoomObjectNoobTubeVisualFiles
{
    /// <summary>Editable n00b-tube visual JSON filename.</summary>
    private const string VisualFileName = "noob-tube.json";

    /// <summary>The n00b-tube as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "n00b-tube", All, VisualId);

    /// <summary>Pictures every n00b-tube frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed n00b-tube pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectPictures Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Enumerates the tube's five frames in their published order: the glass's three, then the opened rows, upper first.</summary>
    private static IEnumerable<NoobTubeDraw> Draws()
    {
        yield return NoobTubeDraw.Intact;
        yield return NoobTubeDraw.Cracking;
        yield return NoobTubeDraw.Opened;
        yield return NoobTubeDraw.UpperRowsOpen;
        yield return NoobTubeDraw.LowerRowsOpen;
    }

    /// <summary>The draw list of <paramref name="draw"/>, which must be one of the glass tube draws.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a glass tube draw.</exception>
    internal static RoomObjectDrawLists.DrawList DrawListOf(RoomObjectDraw draw) =>
        TryGet(draw, out RoomObjectDrawLists.DrawList list) ? list : throw new InvalidDataException($"{draw} is not a glass tube draw.");

    /// <summary>
    /// The draw list of a tube draw (#1862): its block shape written as the glass's one block, or as its twelve-block rows
    /// top row first, the lower rows' cleared glass block before them.
    /// </summary>
    private static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        if (!RoomObjectDrawIdentity.TryShape(NoobTubeObjectDraws.Family, request, out RoomBlockShape? shape))
        {
            list = default;
            return false;
        }
        RoomObjectShapeRun glass = RoomObjectShapeRun.Row(new(0, 0), 1);
        list = RoomObjectShapeRun.DrawList(request, shape, RoomObjectDrawIdentity.FrameOf<NoobTubeDraw>(request, "n00b tube") switch
        {
            NoobTubeDraw.Intact or NoobTubeDraw.Cracking or NoobTubeDraw.Opened => [glass],
            NoobTubeDraw.UpperRowsOpen => TubeRows(0, 1, 2),
            NoobTubeDraw.LowerRowsOpen => [glass, .. TubeRows(3, 4, 5)],
            var frame => throw new InvalidOperationException($"Undefined NoobTubeDraw {frame}."),
        });
        return true;
    }

    /// <summary>The tube's twelve-block <paramref name="rows"/>, each written from the object's column.</summary>
    private static RoomObjectShapeRun[] TubeRows(params int[] rows) => [.. rows.Select(row => RoomObjectShapeRun.Row(new(0, row), 12))];

    /// <summary>Converts a validated tube draw to its artwork catalog ID.</summary>
    private static string VisualId(RoomObjectDraw request) =>
        VisualId(RoomObjectDrawIdentity.FrameOf<NoobTubeDraw>(request, "n00b-tube draw with a visual ID"));

    /// <summary>Names the intact, cracking or opened glass, or the opened upper or lower rows.</summary>
    private static string VisualId(NoobTubeDraw frame) => frame switch
    {
        NoobTubeDraw.Intact => "intact",
        NoobTubeDraw.Cracking => "cracking",
        NoobTubeDraw.Opened => "opened",
        NoobTubeDraw.UpperRowsOpen => "upper-rows-open",
        NoobTubeDraw.LowerRowsOpen => "lower-rows-open",
        _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {frame}."),
    };

    /// <summary>Builds each known tube layout's draw list.</summary>
    private static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            foreach (NoobTubeDraw frame in Draws())
            {
                yield return DrawListOf(NoobTubeObjectDraws.Family.Of(frame));
            }
        }
    }
}
