using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the three elevator-platform frames (#1557), and the published names that key them.</summary>
public static class RoomObjectElevatorPlatformVisualFiles
{
    /// <summary>Family-relative installed JSON filename for the elevator platform's three six-cell artwork frames.</summary>
    public const string VisualFileName = "elevator-platforms.json";

    /// <summary>The elevator-platform as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Elevator platform", DrawLists, VisualId);

    /// <summary>Pictures every elevator-platform frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed elevator-platform pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectElevatorPlatformVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The draw lists of the three platform frames in order.</summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> DrawLists
    {
        get
        {
            foreach (ElevatorPlatformDraw platform in Enum.GetValues<ElevatorPlatformDraw>())
            {
                TryGetDraw(ElevatorPlatformDraws.Family.Of(platform), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Maps a platform draw to its stable artwork identifier.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        RoomObjectVisualIdDefinitions.ElevatorPlatform.TryGetValue(
            RoomObjectDrawIdentity.FrameOf<ElevatorPlatformDraw>(request, "elevator-platform draw list"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined elevator-platform draw list {request}.");

    /// <summary>
    /// The draw list of a platform frame (#1862): its block shape written as its two upper edge blocks, the object's and
    /// the one three right of it, then the four-block row below them.
    /// </summary>
    internal static bool TryGetDraw(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        if (!RoomObjectDrawIdentity.TryShape(ElevatorPlatformDraws.Family, request, out RoomBlockShape? shape))
        {
            list = default;
            return false;
        }
        list = RoomObjectShapeRun.DrawList(request, shape, RoomObjectShapeRun.Row(new(0, 0), 1), RoomObjectShapeRun.Row(new(3, 0), 1),
            RoomObjectShapeRun.Row(new(0, 1), 4));
        return true;
    }
}
