using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the twenty station frames (#1557), and the published names that key them.</summary>
public static class RoomObjectStationVisualFiles
{
    /// <summary>Family-relative installed JSON filename for twenty map, refill, save, and station-access draw layouts.</summary>
    public const string VisualFileName = "stations.json";

    /// <summary>The station as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Station", All, VisualId);

    /// <summary>Pictures every station frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed station pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectStationVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>
    /// The draw list of a station draw (#1862): its block shape written as rows, the object's block first. A display writes
    /// its second block beside or above the first, a save pod its six two-block rows upward, and a retracted map arm its
    /// second block three across.
    /// </summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        if (!StationObjectDraws.TryDescribe(request, out RoomBlockShape? shape))
        {
            list = default;
            return false;
        }
        RoomObjectShapeRun own = RoomObjectShapeRun.Row(new(0, 0), 1);
        list = RoomObjectShapeRun.DrawList(request, shape, RoomObjectDraw.FrameOf<StationDraw>(request, "station") switch
        {
            StationDraw.MapFrame0 or StationDraw.MapFrame1 or StationDraw.MapFrame2 => [own, RoomObjectShapeRun.Row(new(-1, 0), 1)],
            StationDraw.EnergyFrame0 or StationDraw.EnergyFrame1 or StationDraw.EnergyFrame2 or StationDraw.MissileFrame0
                or StationDraw.MissileFrame1 or StationDraw.MissileFrame2 => [own, RoomObjectShapeRun.Row(new(0, -1), 1)],
            StationDraw.SaveIdle or StationDraw.SaveActive or StationDraw.SaveAlternate =>
                [.. Enumerable.Range(0, 6).Select(row => RoomObjectShapeRun.Row(new(0, -row), 2))],
            StationDraw.MapRightRetracted => [own, RoomObjectShapeRun.Row(new(-3, 0), 1)],
            StationDraw.MapLeftRetracted => [own, RoomObjectShapeRun.Row(new(3, 0), 1)],
            StationDraw.MapRightExtended or StationDraw.MapLeftExtended or StationDraw.ResourceRightRetracted
                or StationDraw.ResourceRightExtended or StationDraw.ResourceLeftRetracted or StationDraw.ResourceLeftExtended => [own],
            var frame => throw new InvalidOperationException($"Undefined station draw {frame}."),
        });
        return true;
    }

    /// <summary>
    /// Published ordinal artwork IDs: three numbered frames per map/resource
    /// station, and named save/access states. These are identity selections,
    /// independent of the terrain. Only complete lists have IDs.
    /// </summary>
    internal static string VisualId(RoomObjectDraw request) =>
        RoomObjectDraw.FrameOf<StationDraw>(request, "station draw list") switch
        {
            StationDraw.MapFrame0 => "map-frame-0",
            StationDraw.MapFrame1 => "map-frame-1",
            StationDraw.MapFrame2 => "map-frame-2",
            StationDraw.EnergyFrame0 => "energy-frame-0",
            StationDraw.EnergyFrame1 => "energy-frame-1",
            StationDraw.EnergyFrame2 => "energy-frame-2",
            StationDraw.MissileFrame0 => "missile-frame-0",
            StationDraw.MissileFrame1 => "missile-frame-1",
            StationDraw.MissileFrame2 => "missile-frame-2",
            StationDraw.SaveIdle => "save-idle",
            StationDraw.SaveActive => "save-active-a",
            StationDraw.SaveAlternate => "save-active-b",
            StationDraw.MapRightRetracted => "map-right-retracted",
            StationDraw.MapRightExtended => "map-right-extended",
            StationDraw.MapLeftRetracted => "map-left-retracted",
            StationDraw.MapLeftExtended => "map-left-extended",
            StationDraw.ResourceRightRetracted => "resource-right-retracted",
            StationDraw.ResourceRightExtended => "resource-right-extended",
            StationDraw.ResourceLeftRetracted => "resource-left-retracted",
            StationDraw.ResourceLeftExtended => "resource-left-extended",
            _ => throw new InvalidOperationException($"Undefined station draw list {request}."),
        };

    /// <summary>Calculates the twenty complete station lists in published enumeration order.</summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            foreach (StationDraw frame in RoomObjectVisualIdDefinitions.StationPublishedOrder)
            {
                TryGet(RoomObjectDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }
}
