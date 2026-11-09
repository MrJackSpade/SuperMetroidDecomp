using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;

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
    /// <param name="sourceCartridgeSha256">The cartridge's identity, recorded in the file.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the glass draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, sourceCartridgeSha256, Family);

    /// <summary>Loads the installed glass pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format or source is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledMotherBrainGlassFrames Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Every glass frame's geometry, from the shapes the glass draws by.</summary>
    private static Dictionary<MotherBrainGlassDraw, RoomObjectFrameLayout> Layouts()
    {
        var layouts = new Dictionary<MotherBrainGlassDraw, RoomObjectFrameLayout>();
        foreach (MotherBrainGlassDraw frame in Enum.GetValues<MotherBrainGlassDraw>())
        {
            RoomObjectDraw draw = RoomObjectDraw.Of(frame);
            if (!MotherBrainGlassDraws.TryDescribe(draw, out MotherBrainGlassDraws.Draw shape))
                throw new InvalidOperationException($"Mother Brain glass frame {frame} has no shape.");
            layouts.Add(frame, RoomObjectFrameLayout.Of(draw, shape.RunCount, shape.BlockCount, shape.CollisionAt,
                run => shape.Vertical(run) ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal, shape.Start));
        }
        return layouts;
    }
}
