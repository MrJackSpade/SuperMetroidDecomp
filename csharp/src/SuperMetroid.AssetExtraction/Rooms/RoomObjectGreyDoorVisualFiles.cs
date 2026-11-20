using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the twenty grey-door and shared clear-cap frames (#1557), and the published names that key them.</summary>
public static class RoomObjectGreyDoorVisualFiles
{
    /// <summary>Family-relative JSON filename for twenty grey-door and shared clear-cap layouts.</summary>
    private const string VisualFileName = "grey-doors.json";

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
    private static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            foreach (RoomObjectDraw draw in Draws())
            {
                yield return DrawListOf(draw);
            }
        }
    }

    /// <summary>The draw list of <paramref name="draw"/>, which must be one of the grey door cap draws.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a grey door cap draw.</exception>
    internal static RoomObjectDrawLists.DrawList DrawListOf(RoomObjectDraw draw) =>
        TryGet(draw, out RoomObjectDrawLists.DrawList list) ? list : throw new InvalidDataException($"{draw} is not a grey door cap draw.");

    /// <summary>Builds the draw list of a recognized grey or cleared cap from how its cells collide.</summary>
    private static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        bool found = RoomObjectDrawIdentity.TryFrame<GreyDoorCaps.Draw>(request, out GreyDoorCaps.Draw cap);
        list = found ? RoomObjectShapeRun.DrawList(request, cap.Blocks, RoomObjectShapeRun.Line(cap.LineAxis, cap.Blocks)) : default;
        return found;
    }

    /// <summary>Names a cleared opening or grey cap by direction and what it shows (#1994).</summary>
    private static string VisualId(RoomObjectDraw request)
    {
        if (!RoomObjectDrawIdentity.TryFrame<GreyDoorCaps.Draw>(request, out var draw))
            throw new InvalidDataException($"Grey-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        return draw.IsCleared ? $"clear-{direction}" : $"grey-{direction}-{DoorCapLookNames.VisualName(draw.Look)}";
    }

    /// <summary>The four cleared openings, then each facing's grey cap closed and sliding aside.</summary>
    private static IEnumerable<RoomObjectDraw> Draws()
    {
        foreach (ColoredDoorOrientation orientation in DoorCapOrientations.InTableOrder)
            yield return GreyDoorCaps.Family.Of(GreyDoorCaps.Draw.ClearedOf(orientation));
        foreach (ColoredDoorOrientation orientation in DoorCapOrientations.InTableOrder)
            foreach (DoorCapLook look in DoorCaps.OpeningLooks)
                yield return GreyDoorCaps.Family.Of(new GreyDoorCaps.Draw(orientation, look));
    }
}
