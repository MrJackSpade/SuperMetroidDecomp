using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the eleven frames of Mother Brain's glass (#1557).</summary>
public static class RoomObjectMotherBrainGlassVisualFiles
{
    /// <summary>The installed file holding the initial, damaged, shifted, shattering and cleared glass frames' pictures.</summary>
    public const string VisualFileName = "mother-brain-glass.json";

    /// <summary>The glass as the import pictures it: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<MotherBrainGlassDraw> Family { get; } =
        new(VisualFileName, "Mother Brain glass", Layouts(), RoomObjectVisualIdDefinitions.MotherBrainGlass);

    /// <summary>Pictures the eleven glass frames from the cartridge's draw lists and writes the installed file.</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the glass's geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the glass draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed glass pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledMotherBrainGlassFrames Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>
    /// Every glass frame's geometry as the cartridge writes it: the runs of its draw list, each cell colliding as the
    /// game's block shape for the frame says (#1608). The run form is the cartridge's; the game draws the shape.
    /// </summary>
    private static Dictionary<MotherBrainGlassDraw, RoomObjectFrameLayout> Layouts()
    {
        // The intact glass and a shifted pane's glass block are one-cell rows; every other run is a column. The pane's far
        // columns start three, two and one blocks left of the glass block, lower where their top block is not drawn.
        Offset16<RoomBlocks> origin = Offset16<RoomBlocks>.Zero;
        RoomObjectShapeRun[] square = [RoomObjectShapeRun.Column(origin, 4), RoomObjectShapeRun.Column(new(-3, 0), 4),
            RoomObjectShapeRun.Column(new(-2, 0), 4), RoomObjectShapeRun.Column(new(-1, 0), 4)];
        RoomObjectShapeRun[] pane = [RoomObjectShapeRun.Column(origin, 4), RoomObjectShapeRun.Column(new(-1, 1), 2)];
        var runs = new Dictionary<MotherBrainGlassDraw, RoomObjectShapeRun[]>
        {
            [MotherBrainGlassDraw.Initial] = [RoomObjectShapeRun.Row(origin, 1)],
            [MotherBrainGlassDraw.PaneDamage1] = pane,
            [MotherBrainGlassDraw.PaneDamage2] = pane,
            [MotherBrainGlassDraw.PaneTransition] = [RoomObjectShapeRun.Column(origin, 3)],
            [MotherBrainGlassDraw.ShiftedPane1] = [RoomObjectShapeRun.Row(origin, 1), RoomObjectShapeRun.Column(new(-3, 0), 4),
                RoomObjectShapeRun.Column(new(-2, 1), 2)],
            [MotherBrainGlassDraw.ShiftedPane2] = [RoomObjectShapeRun.Row(origin, 1), RoomObjectShapeRun.Column(new(-3, 1), 3),
                RoomObjectShapeRun.Column(new(-2, 1), 2)],
            [MotherBrainGlassDraw.ShiftedPane3] = [RoomObjectShapeRun.Row(origin, 1), RoomObjectShapeRun.Column(new(-3, 1), 2)],
            [MotherBrainGlassDraw.Shatter1] = [RoomObjectShapeRun.Column(origin, 4), RoomObjectShapeRun.Column(new(-3, 0), 4),
                RoomObjectShapeRun.Column(new(-2, 1), 2), RoomObjectShapeRun.Column(new(-1, 1), 2)],
            [MotherBrainGlassDraw.Shatter2] = square,
            [MotherBrainGlassDraw.Shatter3] = square,
            [MotherBrainGlassDraw.Cleared] = square,
        };

        var layouts = new Dictionary<MotherBrainGlassDraw, RoomObjectFrameLayout>();
        foreach (MotherBrainGlassDraw frame in Enum.GetValues<MotherBrainGlassDraw>())
        {
            RoomObjectDraw draw = RoomObjectDraw.Of(frame);
            if (!MotherBrainGlassDraws.TryDescribe(draw, out RoomBlockShape? shape))
                throw new InvalidOperationException($"Mother Brain glass frame {frame} has no shape.");
            layouts.Add(frame, RoomObjectShapeRun.Layout(draw, shape, runs[frame]));
        }
        return layouts;
    }
}
