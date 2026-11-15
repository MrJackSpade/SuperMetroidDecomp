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

    /// <summary>Enumerates the column open, lowering and closed, followed by four left/right trigger color pairs.</summary>
    private static IEnumerable<RoomObjectDraw> Draws()
    {
        foreach (DownwardGateDraw gate in Enum.GetValues<DownwardGateDraw>())
            yield return DownwardGateObjectDraws.Family.Of(gate);
    }

    /// <summary>Builds the draw list of a recognized column or trigger draw from its geometry.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        bool found = RoomObjectDrawIdentity.TryFrame(request, out DownwardGateDraw gate);
        DownwardGateObjectDraws.Draw draw = DownwardGateObjectDraws.Describe(gate);
        // The column is written as one five-block column, a right trigger as one row of two, and a left trigger as its
        // block then the trigger left of it (#1862).
        list = found
            ? RoomObjectShapeRun.DrawList(request, draw.Blocks, draw.Part switch
            {
                DownwardGatePart.Column => [RoomObjectShapeRun.Column(new(0, 0), 5)],
                DownwardGatePart.RightTrigger => [RoomObjectShapeRun.Row(new(0, 0), 2)],
                DownwardGatePart.LeftTrigger => [RoomObjectShapeRun.Row(new(0, 0), 1), RoomObjectShapeRun.Row(new(-1, 0), 1)],
                _ => throw new InvalidOperationException($"Undefined {nameof(DownwardGatePart)} {draw.Part}."),
            })
            : default;
        return found;
    }

    /// <summary>Maps a draw to its published artwork identity: the column by how far it is lowered, a trigger by its colour and side (#1994).</summary>
    internal static string VisualId(RoomObjectDraw request)
    {
        if (!RoomObjectDrawIdentity.TryFrame(request, out DownwardGateDraw gate))
            throw new InvalidDataException($"Downward gate draw list {request} has no visual ID.");
        return gate switch
        {
            DownwardGateDraw.Open => "column-open",
            DownwardGateDraw.QuarterLowered => "column-quarter-lowered",
            DownwardGateDraw.HalfLowered => "column-half-lowered",
            DownwardGateDraw.ThreeQuartersLowered => "column-three-quarters-lowered",
            DownwardGateDraw.FullyLowered => "column-fully-lowered",
            DownwardGateDraw.Closed => "column-closed",
            DownwardGateDraw.TriggerBlueLeft => "blue-left-trigger",
            DownwardGateDraw.TriggerBlueRight => "blue-right-trigger",
            DownwardGateDraw.TriggerRedLeft => "red-left-trigger",
            DownwardGateDraw.TriggerRedRight => "red-right-trigger",
            DownwardGateDraw.TriggerGreenLeft => "green-left-trigger",
            DownwardGateDraw.TriggerGreenRight => "green-right-trigger",
            DownwardGateDraw.TriggerYellowLeft => "yellow-left-trigger",
            DownwardGateDraw.TriggerYellowRight => "yellow-right-trigger",
            _ => throw new InvalidDataException($"Downward gate draw {request} has no visual ID."),
        };
    }

    /// <summary>Builds every column and trigger draw list from its calculated shape.</summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> All
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
