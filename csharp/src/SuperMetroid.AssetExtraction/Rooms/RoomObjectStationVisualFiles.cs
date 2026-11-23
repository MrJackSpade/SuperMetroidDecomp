using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the twenty station frames (#1557), and the published names that key them.</summary>
public static partial class RoomObjectStationVisualFiles
{
    /// <summary>Family-relative installed JSON filename for twenty map, refill, save, and station-access draw layouts.</summary>
    internal const string VisualFileName = "stations.json";

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
    public static InstalledRoomObjectPictures Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The draw list of <paramref name="draw"/>, which must be one of the station draws.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a station draw.</exception>
    internal static RoomObjectDrawLists.DrawList DrawListOf(RoomObjectDraw draw) =>
        TryGet(draw, out RoomObjectDrawLists.DrawList list) ? list : throw new InvalidDataException($"{draw} is not a station draw.");

    /// <summary>
    /// The draw list of a station draw (#1862): its block shape written as rows, the object's block first. A display writes
    /// its second block beside or above the first, a save pod its six two-block rows upward, and a retracted map arm its
    /// second block three across.
    /// </summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        if (!RoomObjectDrawIdentity.TryShape(StationObjectDraws.Family, request, out RoomBlockShape? shape))
        {
            list = default;
            return false;
        }
        StationDraw frame = RoomObjectDrawIdentity.FrameOf<StationDraw>(request, "station");
        list = RoomObjectShapeRun.DrawList(request, shape, [.. Ensure.Listed(StationFrameDefinitions.Frames, frame).Runs]);
        return true;
    }

    /// <summary>
    /// Published artwork IDs (#1994): each display's dim, lit and bright glow, and the save pod's and the access
    /// arms' states. These are identity selections, independent of the terrain. Only complete lists have IDs.
    /// </summary>
    internal static string VisualId(RoomObjectDraw request) =>
        StationDrawKeys.Installed.Of(RoomObjectDrawIdentity.FrameOf<StationDraw>(request, "station draw list"));

    /// <summary>Calculates the twenty complete station lists in published enumeration order.</summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            foreach (StationDraw frame in RoomObjectVisualIdDefinitions.StationPublishedOrder)
            {
                yield return DrawListOf(StationObjectDraws.Family.Of(frame));
            }
        }
    }
}
