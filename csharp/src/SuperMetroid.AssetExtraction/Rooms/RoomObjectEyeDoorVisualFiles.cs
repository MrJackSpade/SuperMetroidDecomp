using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the twenty-three eye-door frames (#1557), and the published names that key them.</summary>
public static class RoomObjectEyeDoorVisualFiles
{
    /// <summary>JSON filename for editable eye-door eye, middle, bottom, and shared clearing frames.</summary>
    public const string VisualFileName = "eye-doors.json";

    /// <summary>The eye-door as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Eye-door", Editable, VisualId);

    /// <summary>Pictures every eye-door frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed eye-door pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectEyeDoorVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Enumerates both sides' clear, eye, middle, and bottom draws, left side first.</summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            foreach (EyeDoorOrientation side in (EyeDoorOrientation[])[EyeDoorOrientation.Left, EyeDoorOrientation.Right])
            {
                yield return DrawListOf(EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, side)));
                foreach (EyeDoorLook look in EyeDoorLookNames.TableOrder)
                {
                    yield return DrawListOf(EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(look, side)));
                }
            }
        }
    }

    /// <summary>The mirrored clear shares one visual identity, keeping the installed file at 23 frames.</summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> Editable =>
        All.Where(draw => draw.Draw != EyeDoorObjectDrawDefinitions.MirroredOpeningClear);

    /// <summary>The draw list of <paramref name="draw"/>, which must be one of the eye door draws.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a eye door draw.</exception>
    internal static RoomObjectDrawLists.DrawList DrawListOf(RoomObjectDraw draw) =>
        TryGet(draw, out RoomObjectDrawLists.DrawList list) ? list : throw new InvalidDataException($"{draw} is not a eye door draw.");

    /// <summary>Builds the one-run draw list of a recognized eye-door draw from its cells' collision.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        if (!RoomObjectDrawIdentity.TryFrame<EyeDoorDraws.Draw>(request, out var draw)) { list = default; return false; }
        list = RoomObjectShapeRun.DrawList(request, draw.Blocks, RoomObjectShapeRun.Line(draw.Vertical, draw.Blocks));
        return true;
    }

    /// <summary>Gets a stable side and look identifier for an editable draw (#1994).</summary>
    internal static string VisualId(RoomObjectDraw request)
    {
        if (!RoomObjectDrawIdentity.TryFrame<EyeDoorDraws.Draw>(request, out var draw) || request == EyeDoorObjectDrawDefinitions.MirroredOpeningClear)
            throw new InvalidDataException($"Eye-door draw {request} has no visual ID.");
        return draw.Look == EyeDoorLook.Clear ? "left-eye-clear" : $"{(draw.Orientation == EyeDoorOrientation.Left ? "left" : "right")}-{EyeDoorLookNames.VisualName(draw.Look)}";
    }
}
