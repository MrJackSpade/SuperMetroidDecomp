using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the open, half-closed and closed frames of Mother Brain's escape gate (#1557).</summary>
public static class RoomObjectEscapeGateVisualFiles
{
    /// <summary>The installed file holding the gate frames' pictures.</summary>
    public const string VisualFileName = "escape-gate.json";

    /// <summary>The gate as the import pictures it: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<MotherBrainEscapeGateDraw> Family { get; } =
        new(VisualFileName, "Escape-gate", Layouts(), RoomObjectVisualIdDefinitions.MotherBrainEscapeGate);

    /// <summary>Pictures the three gate frames from the cartridge's draw lists and writes the installed file.</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the gate's four solid cells.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the gate draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed gate pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledEscapeGateFrames Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>
    /// Every gate frame's geometry as the cartridge writes it: one four-cell column from the origin, each cell colliding
    /// as the game's block shape for the frame says (#1608).
    /// </summary>
    private static Dictionary<MotherBrainEscapeGateDraw, RoomObjectFrameLayout> Layouts()
    {
        var layouts = new Dictionary<MotherBrainEscapeGateDraw, RoomObjectFrameLayout>();
        foreach (MotherBrainEscapeGateDraw frame in Enum.GetValues<MotherBrainEscapeGateDraw>())
        {
            RoomObjectDraw draw = RoomObjectDraw.Of(frame);
            if (!MotherBrainEscapeGateDraws.TryDescribe(draw, out RoomBlockShape? shape))
                throw new InvalidOperationException($"Escape-gate frame {frame} has no shape.");
            layouts.Add(frame, RoomObjectShapeRun.Layout(draw, shape, RoomObjectShapeRun.Column(Offset16<RoomBlocks>.Zero, 4)));
        }
        return layouts;
    }
}
