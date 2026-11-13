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

    /// <summary>Builds the draw list of a station draw from its calculated cells' geometry and collision.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        if (!StationObjectDraws.TryDescribe(request, out StationObjectDraws.Draw? draw))
        {
            list = default;
            return false;
        }
        list = RoomObjectDrawListExport.Strips(request, draw.RunCount, draw.BlockCount, draw.CollisionAt, draw.Start);
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
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
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
