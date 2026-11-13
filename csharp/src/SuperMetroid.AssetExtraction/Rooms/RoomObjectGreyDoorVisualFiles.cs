using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the twenty grey-door and shared clear-cap frames (#1557), and the published names that key them.</summary>
public static class RoomObjectGreyDoorVisualFiles
{
    /// <summary>Family-relative JSON filename for twenty grey-door and shared clear-cap layouts.</summary>
    public const string VisualFileName = "grey-doors.json";

    /// <summary>The grey-door cap as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Grey-door", All, VisualId);

    /// <summary>Pictures every grey-door cap frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed grey-door cap pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectGreyDoorVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Enumerates all twenty authored cap records for artwork import and export.</summary>
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

    /// <summary>Builds the draw list of a recognized grey or cleared cap from how its cells collide.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        bool found = GreyDoorCaps.TryDescribe(request, out GreyDoorCaps.Draw cap);
        list = found ? RoomObjectDrawListExport.Cap(request, cap.Vertical, cap.CollisionAt)
 : default;
        return found;
    }

    /// <summary>Names a supported clear or grey animation frame by direction and frame number.</summary>
    internal static string VisualId(RoomObjectDraw request)
    {
        if (!GreyDoorCaps.TryDescribe(request, out var draw))
            throw new InvalidDataException($"Grey-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        return draw.Frame is int frame ? $"grey-{direction}-frame-{frame}" : $"clear-{direction}";
    }

    /// <summary>The four cleared openings, then each facing's four grey frames.</summary>
    private static IEnumerable<RoomObjectDraw> Draws()
    {
        for (int orientation = 0; orientation < 4; orientation++)
            yield return RoomObjectDraw.Of(GreyDoorCaps.Draw.ClearedOf(DoorCapOrientations.FromTableIndex(orientation)));
        for (int orientation = 0; orientation < 4; orientation++)
            for (int frame = 0; frame < 4; frame++)
                yield return RoomObjectDraw.Of(new GreyDoorCaps.Draw(DoorCapOrientations.FromTableIndex(orientation), frame));
    }
}
