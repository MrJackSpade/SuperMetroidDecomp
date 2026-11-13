using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the sixteen blue-door cap frames (#1557), and the published names that key them.</summary>
public static class RoomObjectBlueDoorVisualFiles
{
    /// <summary>Editable blue-door cap and shared clear-frame JSON filename.</summary>
    public const string VisualFileName = "blue-doors.json";

    /// <summary>The blue-door cap as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Blue-door", Editable, VisualId);

    /// <summary>Pictures every blue-door cap frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <param name="sourceCartridgeSha256">The cartridge's identity, recorded in the file.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, sourceCartridgeSha256, Family);

    /// <summary>Loads the installed blue-door cap pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format or source is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectBlueDoorVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Sixteen original artwork identities; the four physical aliases share frame zero.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> Editable
    {
        get
        {
            for (int orientation = 0; orientation < 4; orientation++)
                for (int frame = 0; frame < 4; frame++)
                {
                    TryGet(RoomObjectDraw.Of(new BlueDoorCaps.Draw(DoorCapOrientations.FromTableIndex(orientation), frame)), out var draw);
                    yield return draw;
                }
        }
    }

    /// <summary>Builds the draw list of a recognized blue cap from how its cells collide.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        bool found = BlueDoorCaps.TryDescribe(request, out BlueDoorCaps.Draw cap);
        list = found ? RoomObjectDrawListExport.Cap(request, cap.Vertical, cap.CollisionAt) : default;
        return found;
    }

    /// <summary>Names an editable cap frame by direction and opening phase.</summary>
    internal static string VisualId(RoomObjectDraw request)
    {
        if (!BlueDoorCaps.TryDescribe(request, out var draw))
            throw new InvalidDataException($"Blue-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        // The alias shows its picture frame's artwork, so it shares that frame's name.
        return $"{direction}-frame-{draw.Frame ?? BlueDoorObjectDrawDefinitions.AliasPictureFrame}";
    }
}
