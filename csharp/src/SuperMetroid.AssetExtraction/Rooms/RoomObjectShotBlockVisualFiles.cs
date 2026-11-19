using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the nineteen ordinary shot-block frames (#1557), and the published names that key them.</summary>
public static class RoomObjectShotBlockVisualFiles
{
    /// <summary>Family-relative installed JSON filename for all nineteen ordinary shot-block breakup and restoration draws.</summary>
    public const string VisualFileName = "shot-blocks.json";

    /// <summary>The breakup's stages in the installed file's order (#2567): rock, pieces, dust, gone, each in every arrangement.</summary>
    private static readonly IReadOnlyList<BlockBreakupStage> BreakupStages = Ensure.EveryMemberOnce<BlockBreakupStage>(
    [
        BlockBreakupStage.Rock,
        BlockBreakupStage.Pieces,
        BlockBreakupStage.Dust,
        BlockBreakupStage.Gone,
    ]);
    /// <summary>The shot-block as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Shot-block", All, VisualId);

    /// <summary>Pictures every shot-block frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed shot-block pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectShotBlockVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The published artwork identity of a shot-block draw: its shape and breakup stage, or its restoration (#1994).</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a shot-block draw.</exception>
    private static string VisualId(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<ShotBlockObjectDraws.BreakupDraw> { Frame: var breakup } => $"{BlockArrangementKeys.Installed.Of(breakup.Layout)}-{BlockBreakupStageKeys.Installed.Of(breakup.Stage)}",
        RoomObjectFrameDraw<ShotBlockObjectDraws.RestorationDraw> { Frame: var restoration } => $"{BlockArrangementKeys.Installed.Of(restoration.Layout)}-restore",
        _ => throw new InvalidDataException($"{draw} is not a shot-block draw."),
    };

    /// <summary>The draw lists of every breakup and restoration draw, built from the calculated draws.</summary>
    private static IEnumerable<RoomObjectDrawLists.DrawList> All => Calculated.Select(Export);

    /// <summary>The draw list of a shot-block draw: its block shape written in its arrangement's runs (#1862).</summary>
    private static RoomObjectDrawLists.DrawList Export(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<ShotBlockObjectDraws.BreakupDraw> { Frame: var breakup } =>
            RoomObjectShapeRun.DrawList(draw, breakup.Blocks, RoomObjectShapeRun.Arrangement(breakup.Layout)),
        RoomObjectFrameDraw<ShotBlockObjectDraws.RestorationDraw> { Frame: var restoration } =>
            RoomObjectShapeRun.DrawList(draw, restoration.Blocks, RoomObjectShapeRun.Arrangement(restoration.Layout)),
        _ => throw new InvalidOperationException($"{draw} is not a shot-block draw."),
    };

    /// <summary>Yields breakup stages interleaved by shape, followed by the three restoration draws.</summary>
    private static IEnumerable<RoomObjectDraw> Calculated
    {
        get
        {
            foreach (BlockBreakupStage stage in BreakupStages)
            {
                yield return ShotBlockObjectDraws.Breakup(BlockArrangement.Single, stage);
                yield return ShotBlockObjectDraws.Breakup(BlockArrangement.Horizontal, stage);
                yield return ShotBlockObjectDraws.Breakup(BlockArrangement.Vertical, stage);
                yield return ShotBlockObjectDraws.Breakup(BlockArrangement.Square, stage);
            }
            yield return ShotBlockObjectDraws.Restoration(BlockArrangement.Horizontal);
            yield return ShotBlockObjectDraws.Restoration(BlockArrangement.Vertical);
            yield return ShotBlockObjectDraws.Restoration(BlockArrangement.Square);
        }
    }
}
