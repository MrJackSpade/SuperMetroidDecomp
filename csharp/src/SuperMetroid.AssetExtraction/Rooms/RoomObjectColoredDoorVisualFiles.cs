using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the forty-eight yellow, green and red door-cap frames (#1557), and the published names that key them.</summary>
public static class RoomObjectColoredDoorVisualFiles
{
    /// <summary>JSON filename for yellow, green, and red door-cap visual frames.</summary>
    internal const string VisualFileName = "colored-doors.json";

    /// <summary>The cap colours in the installed file's order (#2567): yellow, green, red.</summary>
    private static readonly IReadOnlyList<ColoredDoorColor> Colors = Ensure.EveryMemberOnce<ColoredDoorColor>(
    [
        ColoredDoorColor.Yellow,
        ColoredDoorColor.Green,
        ColoredDoorColor.Red,
    ]);

    /// <summary>The coloured-door cap as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Colored-door", All, VisualId);

    /// <summary>Pictures every coloured-door cap frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed coloured-door cap pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectPictures Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Enumerates yellow, green, and red cap frames in their authored order.</summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            foreach (RoomObjectDraw draw in Draws())
            {
                yield return DrawListOf(draw);
            }
        }
    }

    /// <summary>The draw list of <paramref name="draw"/>, which must be one of the coloured door cap draws.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a coloured door cap draw.</exception>
    internal static RoomObjectDrawLists.DrawList DrawListOf(RoomObjectDraw draw) =>
        TryGet(draw, out RoomObjectDrawLists.DrawList list) ? list : throw new InvalidDataException($"{draw} is not a coloured door cap draw.");

    /// <summary>Builds the draw list of a recognized colored cap from how its cells collide.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        bool found = RoomObjectDrawIdentity.TryFrame<ColoredDoorCaps.Draw>(request, out ColoredDoorCaps.Draw cap);
        list = found ? RoomObjectShapeRun.DrawList(request, cap.Blocks, RoomObjectShapeRun.Line(cap.LineAxis, cap.Blocks)) : default;
        return found;
    }

    /// <summary>Names a compiled cap frame by color, direction, and what it shows for editable artwork (#1994).</summary>
    internal static string VisualId(RoomObjectDraw request)
    {
        if (!RoomObjectDrawIdentity.TryFrame<ColoredDoorCaps.Draw>(request, out var draw))
            throw new InvalidDataException($"Colored-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        string color = draw.Color switch
        {
            ColoredDoorColor.Yellow => "yellow",
            ColoredDoorColor.Green => "green",
            ColoredDoorColor.Red => "red",
            _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorColor)} {draw.Color}."),
        };
        return $"{color}-{direction}-{DoorCapLookNames.VisualName(draw.Look)}";
    }

    /// <summary>Every colour's caps, each facing's cap closed and sliding aside in turn.</summary>
    private static IEnumerable<RoomObjectDraw> Draws()
    {
        foreach (ColoredDoorColor color in Colors)
            foreach (ColoredDoorOrientation orientation in DoorCapOrientations.InTableOrder)
                foreach (DoorCapLook look in DoorCaps.OpeningLooks)
                    yield return ColoredDoorCaps.Family.Of(new ColoredDoorCaps.Draw(color, orientation, look));
    }
}
