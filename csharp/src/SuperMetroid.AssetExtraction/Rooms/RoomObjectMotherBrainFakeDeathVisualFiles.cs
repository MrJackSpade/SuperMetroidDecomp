using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed pictures of the room changes of Mother Brain's fake death (#1557): the wall fill, the escape door, the
/// twelve mural rows, the two fill rows and the six tube clears.
/// </summary>
public static class RoomObjectMotherBrainFakeDeathVisualFiles
{
    /// <summary>The installed file holding the fake-death room frames' pictures.</summary>
    public const string VisualFileName = "mother-brain-fake-death.json";

    /// <summary>The fake-death room changes as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<MotherBrainFakeDeathDraw> Family { get; } =
        new(VisualFileName, "Mother Brain fake-death room", Layouts(), RoomObjectVisualIdDefinitions.MotherBrainFakeDeath);

    /// <summary>Pictures the twenty-two fake-death room frames from the cartridge's draw lists and writes the installed file.</summary>
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
    /// Every fake-death room frame's geometry as the cartridge writes it: the runs of its draw list, each cell colliding as
    /// the game's block shape for the frame says (#1608). The run form is the cartridge's; the game draws the shape.
    /// </summary>
    private static Dictionary<MotherBrainFakeDeathDraw, RoomObjectFrameLayout> Layouts()
    {
        // The mural and solid rows are one thirteen-cell row. The wall writes the origin and the block below it, then the
        // block above; the door writes its left column, then its right. A tube clear writes its column, the middle pair its
        // right column after, and a side tube its one centreward cell after.
        Offset16<RoomBlocks> origin = Offset16<RoomBlocks>.Zero;
        RoomObjectShapeRun[] row = [RoomObjectShapeRun.Row(origin, 13)];
        var runs = new Dictionary<MotherBrainFakeDeathDraw, RoomObjectShapeRun[]>
        {
            [MotherBrainFakeDeathDraw.FillWall] = [RoomObjectShapeRun.Column(origin, 2), RoomObjectShapeRun.Column(new(0, -1), 1)],
            [MotherBrainFakeDeathDraw.EscapeDoor] = [RoomObjectShapeRun.Column(origin, 4), RoomObjectShapeRun.Column(new(1, 0), 4)],
            [MotherBrainFakeDeathDraw.MuralRow1] = row,
            [MotherBrainFakeDeathDraw.MuralRow2] = row,
            [MotherBrainFakeDeathDraw.MuralRow3] = row,
            [MotherBrainFakeDeathDraw.MuralRow4] = row,
            [MotherBrainFakeDeathDraw.MuralRow5] = row,
            [MotherBrainFakeDeathDraw.MuralRow6] = row,
            [MotherBrainFakeDeathDraw.MuralRow7] = row,
            [MotherBrainFakeDeathDraw.MuralRow8] = row,
            [MotherBrainFakeDeathDraw.MuralRow9] = row,
            [MotherBrainFakeDeathDraw.MuralRow10] = row,
            [MotherBrainFakeDeathDraw.MuralRow11] = row,
            [MotherBrainFakeDeathDraw.MuralRow12] = row,
            [MotherBrainFakeDeathDraw.UnusedFirstSolidRow] = row,
            [MotherBrainFakeDeathDraw.UnusedSecondSolidRow] = row,
            [MotherBrainFakeDeathDraw.ClearCeilingBlock] = [RoomObjectShapeRun.Column(origin, 2)],
            [MotherBrainFakeDeathDraw.ClearCeilingTube] = [RoomObjectShapeRun.Column(origin, 5)],
            [MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube] = [RoomObjectShapeRun.Column(origin, 4)],
            [MotherBrainFakeDeathDraw.ClearBottomMiddleTubes] = [RoomObjectShapeRun.Column(origin, 7), RoomObjectShapeRun.Column(new(1, 0), 7)],
            [MotherBrainFakeDeathDraw.ClearBottomLeftTube] = [RoomObjectShapeRun.Column(origin, 5), RoomObjectShapeRun.Row(new(1, 0), 1)],
            [MotherBrainFakeDeathDraw.ClearBottomRightTube] = [RoomObjectShapeRun.Column(origin, 5), RoomObjectShapeRun.Row(new(-1, 0), 1)],
        };

        var layouts = new Dictionary<MotherBrainFakeDeathDraw, RoomObjectFrameLayout>();
        foreach (MotherBrainFakeDeathDraw frame in Enum.GetValues<MotherBrainFakeDeathDraw>())
        {
            RoomObjectDraw draw = RoomObjectDraw.Of(frame);
            if (!MotherBrainFakeDeathDraws.TryDescribe(draw, out RoomBlockShape? shape))
                throw new InvalidOperationException($"Mother Brain fake-death frame {frame} has no shape.");
            layouts.Add(frame, RoomObjectShapeRun.Layout(draw, shape, runs[frame]));
        }
        return layouts;
    }
}
