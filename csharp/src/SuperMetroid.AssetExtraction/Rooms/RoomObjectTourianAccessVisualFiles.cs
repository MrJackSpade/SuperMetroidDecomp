using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Tourian entrance access-floor visuals.</summary>
public static class RoomObjectTourianAccessVisualFiles
{
    /// <summary>Editable Tourian entrance access-floor visual JSON filename.</summary>
    public const string VisualFileName = "tourian-access.json";

    /// <summary>The stages of a row's crumble the floor draws, in their published order (#2567): rock, pieces, dust.</summary>
    private static readonly BlockBreakupStage[] CrumbleStages = [BlockBreakupStage.Rock, BlockBreakupStage.Pieces, BlockBreakupStage.Dust];

    /// <summary>The Tourian access floor as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Tourian access", All, VisualId);

    /// <summary>Pictures every Tourian access floor frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Tourian access floor pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static RoomObjectTourianAccessVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The draw lists of every Tourian access draw, in the cartridge's order: the empty row, the crumble's stages, the clear.</summary>
    private static IEnumerable<RoomObjectDrawLists.DrawList> All =>
        [Compile(TourianAccessDraw.EmptyRow), .. CrumbleStages.Select(CompileCrumble),
            Compile(TourianAccessDraw.Clear)];

    /// <summary>Gets the stable artwork identity of a Tourian access draw: a crumble stage's, or the empty row's or the clear's.</summary>
    internal static string VisualId(RoomObjectDraw draw) =>
        RoomObjectDrawIdentity.TryFrame(draw, out TourianAccessDraws.CrumbleDraw crumble)
            ? CrumbleVisualId(crumble.Stage)
            : VisualId(RoomObjectDrawIdentity.FrameOf<TourianAccessDraw>(draw, "Tourian access"));

    /// <summary>Names the empty row or the six-row clear frame for visual files.</summary>
    internal static string VisualId(TourianAccessDraw draw) => TourianAccessDrawKeys.Installed.Of(draw);

    /// <summary>Names a drawn stage of a row's crumble for visual files (#2291): the crumble's word, then the stage's key.</summary>
    private static string CrumbleVisualId(BlockBreakupStage stage) =>
        TourianAccessObjectDrawDefinitions.CrumbleShapes.ContainsKey(stage) ? "crumble-" + BlockBreakupStageKeys.Installed.Of(stage)
            : throw new InvalidOperationException($"A Tourian access row's crumble draws no stage {stage}.");

    /// <summary>The draw list of a Tourian access draw: its block shape written one four-block row at a time, top row first (#1862).</summary>
    private static RoomObjectDrawLists.DrawList Compile(TourianAccessDraw draw) =>
        Compile(TourianAccessDraws.Family.Of(draw), TourianAccessDraws.Family, draw == TourianAccessDraw.Clear ? 6 : 1);

    /// <summary>The draw list of a drawn stage of a row's crumble: one four-block row (#2291).</summary>
    private static RoomObjectDrawLists.DrawList CompileCrumble(BlockBreakupStage stage) =>
        Compile(TourianAccessDraws.Crumble(stage), TourianAccessDraws.CrumbleFamily, 1);

    /// <summary>The draw list of a frame of <paramref name="family"/>: its block shape written one four-block row at a time, top row first.</summary>
    private static RoomObjectDrawLists.DrawList Compile<TFrame>(RoomObjectDraw request, RoomObjectDrawFamily<TFrame> family, int rows) where TFrame : struct
    {
        if (!RoomObjectDrawIdentity.TryShape(family, request, out RoomBlockShape? shape))
            throw new InvalidOperationException($"Tourian access frame {request} has no shape.");
        return RoomObjectShapeRun.DrawList(request, shape,
            [.. Enumerable.Range(0, rows).Select(row => RoomObjectShapeRun.Row(new(0, row), 4))]);
    }
}
