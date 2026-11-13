using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
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
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (bool left in new[] { true, false })
            {
                TryGet(RoomObjectDraw.Of(new EyeDoorDraws.Draw(EyeDoorDraws.Component.Clear, 0, left)), out var clear);
                yield return clear;
                foreach (EyeDoorDraws.Component part in new[] { EyeDoorDraws.Component.Eye, EyeDoorDraws.Component.Middle, EyeDoorDraws.Component.Bottom })
                    for (int frame = 0; frame < EyeDoorObjectDrawDefinitions.FrameCounts[part]; frame++)
                    {
                        TryGet(RoomObjectDraw.Of(new EyeDoorDraws.Draw(part, frame, left)), out var value);
                        yield return value;
                    }
            }
        }
    }

    /// <summary>The mirrored clear shares one visual identity, keeping the installed file at 23 frames.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> Editable =>
        All.Where(draw => draw.Draw != EyeDoorObjectDrawDefinitions.MirroredOpeningClear);

    /// <summary>Builds the one-run draw list of a recognized eye-door draw from its cells' collision.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        if (!EyeDoorDraws.TryDescribe(request, out var draw)) { list = default; return false; }
        var collisions = new RoomCollisionType[draw.BlockCount];
        for (int cell = 0; cell < collisions.Length; cell++) collisions[cell] = draw.CollisionAt(cell);
        list = RoomObjectDrawListExport.SingleRun(request, draw.Vertical ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal, collisions);
        return true;
    }

    /// <summary>Gets a stable side, component, and frame identifier for an editable draw.</summary>
    internal static string VisualId(RoomObjectDraw request)
    {
        if (!EyeDoorDraws.TryDescribe(request, out var draw) || request == EyeDoorObjectDrawDefinitions.MirroredOpeningClear)
            throw new InvalidDataException($"Eye-door draw {request} has no visual ID.");
        string component = draw.Part switch
        {
            EyeDoorDraws.Component.Clear => "clear",
            EyeDoorDraws.Component.Eye => "eye",
            EyeDoorDraws.Component.Middle => "middle",
            EyeDoorDraws.Component.Bottom => "bottom",
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorDraws.Component)} {draw.Part}."),
        };
        return draw.Part == EyeDoorDraws.Component.Clear ? "left-eye-clear" : $"{(draw.Left ? "left" : "right")}-{component}-frame-{draw.Frame}";
    }
}
