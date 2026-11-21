using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Kraid ceiling and spike visual blocks, and the editable identities that name them.</summary>
public static class RoomObjectKraidVisualFiles
{
    /// <summary>Family-relative installed JSON filename for Kraid's ten ceiling and spike layouts, including the fifteen- and twenty-two-block clears.</summary>
    internal const string VisualFileName = "kraid-room.json";

    /// <summary>The stages of a crumbling block's breakup the room draws, in their published order; the last stage is the picture its draw leaves.</summary>
    private static readonly BlockBreakupStage[] CrumbleStages = [BlockBreakupStage.Rock, BlockBreakupStage.Pieces, BlockBreakupStage.Dust];

    /// <summary>The ceiling and spike frames in their published order after the crumble's stages (#2567): the ceiling's background, the spikes' background, then the two clears.</summary>
    private static readonly IReadOnlyList<KraidRoomDraw> RoomFrames = Ensure.EveryMemberOnce<KraidRoomDraw>(
    [
        KraidRoomDraw.CeilingBackgroundLeftEdge,
        KraidRoomDraw.CeilingBackgroundOddColumn,
        KraidRoomDraw.CeilingBackgroundEvenColumn,
        KraidRoomDraw.SpikeBackgroundLeftColumn,
        KraidRoomDraw.SpikeBackgroundRightColumn,
        KraidRoomDraw.ClearCeiling,
        KraidRoomDraw.ClearSpikes,
    ]);

    /// <summary>The Kraid room as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Kraid room", All, VisualId);

    /// <summary>Pictures every Kraid room frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Kraid room pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectPictures Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Gets the stable artwork identifier of a Kraid room draw: a crumble stage's, or a picture or clear's.</summary>
    /// <exception cref="InvalidDataException">The draw is neither a Kraid room crumble nor a Kraid room draw.</exception>
    internal static string VisualId(RoomObjectDraw request) =>
        RoomObjectDrawIdentity.TryFrame(request, out KraidRoomObjectDraws.CrumbleDraw crumble)
            ? CrumbleVisualId(crumble.Stage)
            : VisualId(RoomObjectDrawIdentity.FrameOf<KraidRoomDraw>(request, "Kraid room draw with a visual ID"));

    /// <summary>Maps a crumbling block's breakup stage to its stable artwork identifier.</summary>
    private static string CrumbleVisualId(BlockBreakupStage stage) => stage switch
    {
        BlockBreakupStage.Rock => "crumble-rock",
        BlockBreakupStage.Pieces => "crumble-pieces",
        BlockBreakupStage.Dust => "crumble-dust",
        BlockBreakupStage.Gone => throw new InvalidOperationException("A Kraid room block's crumble draws no last stage: it leaves its background."),
        _ => throw new InvalidOperationException($"Undefined {nameof(BlockBreakupStage)} {stage}."),
    };

    /// <summary>Maps a named ceiling or spike draw to its stable artwork identifier.</summary>
    internal static string VisualId(KraidRoomDraw draw) => draw switch
    {
        KraidRoomDraw.CeilingBackgroundLeftEdge => "ceiling-background-left-edge",
        KraidRoomDraw.CeilingBackgroundOddColumn => "ceiling-background-odd-column",
        KraidRoomDraw.CeilingBackgroundEvenColumn => "ceiling-background-even-column",
        KraidRoomDraw.SpikeBackgroundLeftColumn => "spike-background-left-column",
        KraidRoomDraw.SpikeBackgroundRightColumn => "spike-background-right-column",
        KraidRoomDraw.ClearCeiling => "clear-ceiling",
        KraidRoomDraw.ClearSpikes => "clear-spikes",
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidRoomDraw)} {draw}."),
    };

    /// <summary>
    /// Every Kraid room frame's draw list as the cartridge writes it, the crumble's stages first: one run along the row from
    /// the object's block, each cell colliding as the game's block shape for the frame says (#1608). The run form is the
    /// cartridge's; the game draws the shape.
    /// </summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> All =>
        CrumbleStages.Select(stage => SingleRow(KraidRoomObjectDraws.Crumble(stage), KraidRoomObjectDraws.CrumbleFamily))
            .Concat(RoomFrames.Select(frame => SingleRow(KraidRoomObjectDraws.Family.Of(frame), KraidRoomObjectDraws.Family)));

    /// <summary>The draw list of a frame of <paramref name="family"/>: one run along the row its shape covers.</summary>
    /// <exception cref="InvalidOperationException">The frame has no shape.</exception>
    private static RoomObjectDrawLists.DrawList SingleRow<TFrame>(RoomObjectDraw draw, RoomObjectDrawFamily<TFrame> family) where TFrame : struct
    {
        if (!RoomObjectDrawIdentity.TryShape(family, draw, out RoomBlockShape? shape))
            throw new InvalidOperationException($"Kraid room frame {draw} has no shape.");
        var collisions = new RoomCollisionType[shape.Cells.Count];
        for (int cell = 0; cell < collisions.Length; cell++) collisions[cell] = shape.CollisionAt(new Offset16<RoomBlocks>(cell, 0));
        return RoomObjectDrawListExport.SingleRun(draw, RoomObjectRunDirection.Horizontal, collisions);
    }
}
