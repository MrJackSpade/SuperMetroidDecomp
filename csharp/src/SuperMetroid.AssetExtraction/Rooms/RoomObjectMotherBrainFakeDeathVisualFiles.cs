using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed pictures of the room changes of Mother Brain's fake death (#1557): the wall fill, the escape door, the
/// twelve mural rows and the six tube clears.
/// </summary>
public static class RoomObjectMotherBrainFakeDeathVisualFiles
{
    /// <summary>The installed file holding the fake-death room frames' pictures.</summary>
    private const string VisualFileName = "mother-brain-fake-death.json";

    /// <summary>
    /// The tube clears in the installed file's order after the wall, the door and the mural rows (#2567): the ceiling's,
    /// then the bottom's middle, left and right.
    /// </summary>
    private static readonly IReadOnlyList<MotherBrainFakeDeathDraw> TubeClears =
    [
        MotherBrainFakeDeathDraw.ClearCeilingBlock,
        MotherBrainFakeDeathDraw.ClearCeilingTube,
        MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube,
        MotherBrainFakeDeathDraw.ClearBottomMiddleTubes,
        MotherBrainFakeDeathDraw.ClearBottomLeftTube,
        MotherBrainFakeDeathDraw.ClearBottomRightTube,
    ];

    /// <summary>The fake-death room changes as the import pictures them: every frame's geometry and published name, by draw.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } = CreateFamily();

    /// <summary>Pictures the twenty fake-death room frames from the cartridge's draw lists and writes the installed file.</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the frames draw by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed fake-death room pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledMotherBrainFakeDeathFrames Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>
    /// Every fake-death room frame's geometry as the cartridge writes it, and its published name, in the installed file's
    /// order: the wall and the door, the mural's rows top to bottom, then the tube clears. Each frame's geometry is the runs
    /// of its draw list, each cell colliding as the game's block shape for the frame says (#1608). The run form is the
    /// cartridge's; the game draws the shape.
    /// </summary>
    private static RoomObjectFrameFamily<RoomObjectDraw> CreateFamily()
    {
        // A mural row is one thirteen-cell row. The wall writes the origin and the block below it, then the block above;
        // the door writes its left column, then its right. A tube clear writes its column, the middle pair its right column
        // after, and a side tube its one centreward cell after.
        Offset16<RoomBlocks> origin = Offset16<RoomBlocks>.Zero;
        var runs = new Dictionary<MotherBrainFakeDeathDraw, RoomObjectShapeRun[]>
        {
            [MotherBrainFakeDeathDraw.FillWall] = [RoomObjectShapeRun.Column(origin, 2), RoomObjectShapeRun.Column(new(0, -1), 1)],
            [MotherBrainFakeDeathDraw.EscapeDoor] = [RoomObjectShapeRun.Column(origin, 4), RoomObjectShapeRun.Column(new(1, 0), 4)],
            [MotherBrainFakeDeathDraw.ClearCeilingBlock] = [RoomObjectShapeRun.Column(origin, 2)],
            [MotherBrainFakeDeathDraw.ClearCeilingTube] = [RoomObjectShapeRun.Column(origin, 5)],
            [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] = [RoomObjectShapeRun.Column(origin, 4)],
            [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] = [RoomObjectShapeRun.Column(origin, 7), RoomObjectShapeRun.Column(new(1, 0), 7)],
            [MotherBrainFakeDeathDraw.ClearBottomLeftTube] = [RoomObjectShapeRun.Column(origin, 5), RoomObjectShapeRun.Row(new(1, 0), 1)],
            [MotherBrainFakeDeathDraw.ClearBottomRightTube] = [RoomObjectShapeRun.Column(origin, 5), RoomObjectShapeRun.Row(new(-1, 0), 1)],
        };
        RoomObjectShapeRun[] muralRow = [RoomObjectShapeRun.Row(origin, 13)];

        var layouts = new List<RoomObjectFamilyFrame<RoomObjectDraw>>();
        void Add<TFrame>(RoomObjectDrawFamily<TFrame> family, RoomObjectFrameDraw<TFrame> draw, RoomObjectShapeRun[] frameRuns, string id)
            where TFrame : struct
        {
            if (!RoomObjectDrawIdentity.TryShape(family, draw, out RoomBlockShape? shape))
                throw new InvalidOperationException($"Mother Brain fake-death frame {draw} has no shape.");
            layouts.Add(new(draw, RoomObjectShapeRun.Layout(draw, shape, frameRuns), id));
        }
        void AddFrame(MotherBrainFakeDeathDraw frame) =>
            Add(MotherBrainFakeDeathDraws.Family, MotherBrainFakeDeathDraws.Family.Of(frame), runs[frame], MotherBrainFakeDeathDrawKeys.Installed.Of(frame));

        AddFrame(MotherBrainFakeDeathDraw.FillWall);
        AddFrame(MotherBrainFakeDeathDraw.EscapeDoor);
        for (int row = 0; row < MotherBrainMuralDefinitions.RowCount; row++)
            Add(MotherBrainFakeDeathDraws.MuralRowFamily, MotherBrainFakeDeathDraws.MuralRow(row), muralRow, MuralRowId(row));
        foreach (MotherBrainFakeDeathDraw frame in TubeClears)
            AddFrame(frame);
        return new(VisualFileName, "Mother Brain fake-death room", layouts);
    }

    /// <summary>The published name of the mural's row <paramref name="row"/>, counted from its top row: its row number from one.</summary>
    private static string MuralRowId(int row) => $"mural-row-{row + 1}";
}
