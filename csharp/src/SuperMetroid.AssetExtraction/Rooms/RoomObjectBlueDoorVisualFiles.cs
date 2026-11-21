using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the sixteen blue-door cap frames (#1557), and the published names that key them.</summary>
public static class RoomObjectBlueDoorVisualFiles
{
    /// <summary>
    /// The looks that show another look's artwork, with the look whose artwork each shows: the closed cap drawn solid shows
    /// the closed cap's, so it shares that frame's name.
    /// </summary>
    private static readonly Dictionary<DoorCapLook, DoorCapLook> SharedPictures = new Dictionary<DoorCapLook, DoorCapLook>
    {
        [DoorCapLook.ClosedSolid] = DoorCapLook.Closed,
    };

    /// <summary>Editable blue-door cap and shared clear-frame JSON filename.</summary>
    internal const string VisualFileName = "blue-doors.json";

    /// <summary>The blue-door cap as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Blue-door", Editable, VisualId);

    /// <summary>Pictures every blue-door cap frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>
    /// Loads the installed blue-door cap pictures as the player left them, without the cartridge, with each facing's closed
    /// cap drawn solid showing that facing's closed cap.
    /// </summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectPictures Load(string directory) =>
        new(WithClosedSolidCaps(RoomObjectFrameImageFiles.Load(directory, Family)));

    /// <summary>
    /// The installed caps' pictures and, under each facing's closed cap drawn solid, the picture of that facing's closed
    /// cap, which it shows (#1627).
    /// </summary>
    private static RoomObjectFrameImages<RoomObjectDraw> WithClosedSolidCaps(RoomObjectFrameImages<RoomObjectDraw> caps)
    {
        var pictures = new Dictionary<RoomObjectDraw, RoomObjectFrameImage>(caps.All);
        foreach (ColoredDoorOrientation orientation in Enum.GetValues<ColoredDoorOrientation>())
        {
            RoomObjectDraw solid = BlueDoorCaps.ClosedSolid(orientation);
            pictures.Add(solid, caps.Image(BlueDoorCaps.VisualSource(solid)));
        }
        return new RoomObjectFrameImages<RoomObjectDraw>(pictures);
    }

    /// <summary>Sixteen original artwork identities; each facing's closed cap drawn solid shows its closed cap's.</summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> Editable
    {
        get
        {
            foreach (ColoredDoorOrientation orientation in DoorCapOrientations.InTableOrder)
                foreach (DoorCapLook look in DoorCaps.OpeningLooks)
                {
                    yield return DrawListOf(BlueDoorCaps.Family.Of(new BlueDoorCaps.Draw(orientation, look)));
                }
        }
    }

    /// <summary>The draw list of <paramref name="draw"/>, which must be one of the blue door cap draws.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a blue door cap draw.</exception>
    internal static RoomObjectDrawLists.DrawList DrawListOf(RoomObjectDraw draw) =>
        TryGet(draw, out RoomObjectDrawLists.DrawList list) ? list : throw new InvalidDataException($"{draw} is not a blue door cap draw.");

    /// <summary>Builds the draw list of a recognized blue cap from how its cells collide.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        bool found = RoomObjectDrawIdentity.TryFrame<BlueDoorCaps.Draw>(request, out BlueDoorCaps.Draw cap);
        list = found ? RoomObjectShapeRun.DrawList(request, cap.Blocks, RoomObjectShapeRun.Line(cap.LineAxis, cap.Blocks)) : default;
        return found;
    }

    /// <summary>Names an editable cap frame by direction and what it shows (#1994).</summary>
    internal static string VisualId(RoomObjectDraw request)
    {
        if (!RoomObjectDrawIdentity.TryFrame<BlueDoorCaps.Draw>(request, out var draw))
            throw new InvalidDataException($"Blue-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        DoorCapLook shown = SharedPictures.TryGetValue(draw.Look, out DoorCapLook picture) ? picture : draw.Look;
        return $"{direction}-{DoorCapLookNames.VisualName(shown)}";
    }

}
