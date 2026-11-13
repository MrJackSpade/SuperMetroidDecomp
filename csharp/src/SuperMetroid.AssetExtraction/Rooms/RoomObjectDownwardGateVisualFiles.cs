using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the fourteen downward-gate column and trigger frames (#1557), and the published names that key them.</summary>
public static class RoomObjectDownwardGateVisualFiles
{
    /// <summary>Family-relative installed JSON filename for six downward-gate column frames and eight left/right trigger layouts.</summary>
    public const string VisualFileName = "downward-gates.json";

    /// <summary>The downward-gate as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Downward gate", All, VisualId);

    /// <summary>Pictures every downward-gate frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed downward-gate pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectDownwardGateVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Enumerates six column stages followed by four left/right trigger color pairs.</summary>
    private static IEnumerable<RoomObjectDraw> Draws()
    {
        foreach (DownwardGateDraw gate in Enum.GetValues<DownwardGateDraw>())
            yield return RoomObjectDraw.Of(gate);
    }

    /// <summary>Builds the draw list of a recognized column or trigger draw from its geometry.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        bool found = DownwardGateObjectDraws.TryDescribe(request, out DownwardGateObjectDraws.Draw draw);
        list = found
            ? RoomObjectDrawListExport.Runs(request, draw.RunCount, draw.BlockCount, draw.CollisionAt,

                _ => draw.Column ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal, draw.Start)
            : default;
        return found;
    }

    /// <summary>Maps a draw to its published artwork identity, preserving historic green/red labels.</summary>
    internal static string VisualId(RoomObjectDraw request)
    {
        if (!DownwardGateObjectDraws.TryDescribe(request, out var draw))
            throw new InvalidDataException($"Downward gate draw list {request} has no visual ID.");
        if (draw.Column) return $"column-frame-{draw.Frame}";
        // Keep published artwork IDs: the historic green/red names differ from order.
        string color = draw.Frame switch { 0 => "blue", 1 => "green", 2 => "red", _ => "yellow" };
        return $"{color}-{(draw.Right ? "right" : "left")}-trigger";
    }

    /// <summary>Builds every column and trigger draw list from its calculated shape.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (RoomObjectDraw draw in Draws())
            {
                TryGet(draw, out var list);
                yield return list;
            }
        }
    }
}
