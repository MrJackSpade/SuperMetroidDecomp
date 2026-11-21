using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the three Chozo statue terrain frames (#1557), and the published names that key them.</summary>
public static class RoomObjectChozoStatueVisualFiles
{
    /// <summary>
    /// The strips both slope-access frames are written as: the ledge, the fill five rows below it, and the step below the
    /// fill's sixth block, one row at a time.
    /// </summary>
    private static readonly RoomObjectShapeRun[] SlopeAccessStrips =
    [
        RoomObjectShapeRun.Row(new(0, 0), 14), RoomObjectShapeRun.Row(new(0, 5), 9), RoomObjectShapeRun.Row(new(5, 6), 2),
        RoomObjectShapeRun.Row(new(5, 7), 1), RoomObjectShapeRun.Row(new(5, 8), 1),
    ];

    /// <summary>Each statue frame's strips, where each starts and how many blocks long: the cleared hand is written as its one block.</summary>
    private static readonly Dictionary<ChozoStatueDraw, RoomObjectShapeRun[]> Strips =
        new Dictionary<ChozoStatueDraw, RoomObjectShapeRun[]>
        {
            [ChozoStatueDraw.ClearSlopeAccess] = SlopeAccessStrips,
            [ChozoStatueDraw.BlockSlopeAccess] = SlopeAccessStrips,
            [ChozoStatueDraw.LowerNorfairClearedHand] = [RoomObjectShapeRun.Row(new(0, 0), 1)],
        };

    /// <summary>JSON filename for the cleared Chozo hand and Wrecked Ship slope-access appearances.</summary>
    private const string VisualFileName = "chozo-statues.json";

    /// <summary>The Chozo statue as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Chozo statue", All, VisualId);

    /// <summary>Pictures every Chozo statue frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Chozo statue pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectPictures Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Enumerates the three supported physical draw identities for artwork export.</summary>
    private static IEnumerable<RoomObjectDraw> Draws()
    {
        yield return ChozoStatueDraws.Family.Of(ChozoStatueDraw.LowerNorfairClearedHand);
        yield return ChozoStatueDraws.Family.Of(ChozoStatueDraw.ClearSlopeAccess);
        yield return ChozoStatueDraws.Family.Of(ChozoStatueDraw.BlockSlopeAccess);
    }

    /// <summary>The draw lists of the three statue draws.</summary>
    private static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            foreach (RoomObjectDraw draw in Draws())
            {
                yield return DrawListOf(draw);
            }
        }
    }

    /// <summary>The draw list of <paramref name="draw"/>, which must be one of the Chozo statue draws.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a Chozo statue draw.</exception>
    internal static RoomObjectDrawLists.DrawList DrawListOf(RoomObjectDraw draw) =>
        TryGet(draw, out RoomObjectDrawLists.DrawList list) ? list : throw new InvalidDataException($"{draw} is not a Chozo statue draw.");

    /// <summary>
    /// The draw list of a statue frame as the cartridge writes it: its horizontal strips in write order, each cell
    /// colliding as the game's block shape for the frame says (#1622). The run form is the cartridge's; the game draws
    /// the shape.
    /// </summary>
    private static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        if (!RoomObjectDrawIdentity.TryShape(ChozoStatueDraws.Family, request, out RoomBlockShape? described))
        {
            list = default;
            return false;
        }
        list = RoomObjectDrawListExport.Runs(request, described,
            Ensure.Listed(Strips, RoomObjectDrawIdentity.FrameOf<ChozoStatueDraw>(request, "Chozo statue")));
        return true;
    }

    /// <summary>Names the cleared hand or blocked/clear slope layout without changing collision ownership.</summary>
    private static string VisualId(RoomObjectDraw request) =>
        ChozoStatueDrawKeys.Installed.Of(RoomObjectDrawIdentity.FrameOf<ChozoStatueDraw>(request, "Chozo statue draw with a visual ID"));
}
