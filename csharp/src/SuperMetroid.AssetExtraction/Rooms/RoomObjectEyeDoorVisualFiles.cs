using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the twenty-three eye-door frames (#1557), and the published names that key them.</summary>
public static class RoomObjectEyeDoorVisualFiles
{
    /// <summary>JSON filename for editable eye-door eye, middle, bottom, and shared clearing frames.</summary>
    private const string VisualFileName = "eye-doors.json";

    /// <summary>The installed name of each side, as the published keys spell it.</summary>
    private static readonly IReadOnlyDictionary<EyeDoorOrientation, string> SideNames = new Dictionary<EyeDoorOrientation, string>
    {
        [EyeDoorOrientation.Left] = "left",
        [EyeDoorOrientation.Right] = "right",
    };

    /// <summary>
    /// The frames that show another frame's picture, with the frame whose picture each shows: the mirrored opening clear
    /// shows the left clear's, so the installed file holds 23 frames.
    /// </summary>
    private static readonly Dictionary<RoomObjectDraw, RoomObjectDraw> SharedPictures = new Dictionary<RoomObjectDraw, RoomObjectDraw>
    {
        [EyeDoorObjectDrawDefinitions.MirroredOpeningClear] = EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Left)),
    };

    /// <summary>
    /// The published key of each frame with a picture of its own (#1994): the shared clear picture once, then each side's
    /// looks by side and look.
    /// </summary>
    private static readonly Dictionary<RoomObjectDraw, string> VisualKeys = PublishedKeys();

    /// <summary>The eye-door as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Eye-door", Editable, VisualId);

    /// <summary>Pictures every eye-door frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>
    /// Loads the installed eye-door pictures as the player left them, without the cartridge, with the right door's opening
    /// clear showing the shared clear mirrored left to right.
    /// </summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectPictures Load(string directory) =>
        new(WithMirroredOpeningClear(RoomObjectFrameImageFiles.Load(directory, Family)));

    /// <summary>The installed frames' pictures and, under the mirrored opening clear, the shared clear's picture mirrored left to right.</summary>
    private static RoomObjectFrameImages<RoomObjectDraw> WithMirroredOpeningClear(RoomObjectFrameImages<RoomObjectDraw> frames)
    {
        var pictures = new Dictionary<RoomObjectDraw, RoomObjectFrameImage>(frames.All);
        pictures.Add(EyeDoorObjectDrawDefinitions.MirroredOpeningClear, frames.Image(EyeDoorObjectDrawDefinitions.SharedClear).MirroredHorizontally());
        return new RoomObjectFrameImages<RoomObjectDraw>(pictures);
    }

    /// <summary>Enumerates both sides' clear, eye, middle, and bottom draws, left side first.</summary>
    private static IEnumerable<RoomObjectDrawLists.DrawList> All
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

    /// <summary>The frames with a picture of their own: a frame showing another's picture is that frame's in the installed file.</summary>
    private static IEnumerable<RoomObjectDrawLists.DrawList> Editable =>
        All.Where(draw => !SharedPictures.ContainsKey(draw.Draw));

    /// <summary>The draw list of <paramref name="draw"/>, which must be one of the eye door draws.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a eye door draw.</exception>
    internal static RoomObjectDrawLists.DrawList DrawListOf(RoomObjectDraw draw) =>
        TryGet(draw, out RoomObjectDrawLists.DrawList list) ? list : throw new InvalidDataException($"{draw} is not a eye door draw.");

    /// <summary>Builds the one-run draw list of a recognized eye-door draw from its cells' collision.</summary>
    private static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        if (!RoomObjectDrawIdentity.TryFrame<EyeDoorDraws.Draw>(request, out var draw)) { list = default; return false; }
        list = RoomObjectShapeRun.DrawList(request, draw.Blocks, RoomObjectShapeRun.Line(draw.LineAxis, draw.Blocks));
        return true;
    }

    /// <summary>Gets a stable side and look identifier for an editable draw (#1994).</summary>
    private static string VisualId(RoomObjectDraw request) => VisualKeys.TryGetValue(request, out string? key) ? key
        : throw new InvalidDataException($"Eye-door draw {request} has no visual ID.");

    /// <summary>The published key of the clear picture both sides' clear frames show.</summary>
    private const string SharedClearKey = "left-eye-clear";

    /// <summary>Spells each frame's published key: the shared clear picture's, which both sides' clear frames show, then each side's looks'.</summary>
    private static Dictionary<RoomObjectDraw, string> PublishedKeys()
    {
        var keys = new Dictionary<RoomObjectDraw, string>
        {
            [EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Left))] = SharedClearKey,
            [EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(EyeDoorLook.Clear, EyeDoorOrientation.Right))] = SharedClearKey,
        };
        foreach ((EyeDoorOrientation side, string sideName) in SideNames)
        {
            foreach (EyeDoorLook look in EyeDoorLookNames.TableOrder)
                keys.Add(EyeDoorDraws.Family.Of(new EyeDoorDraws.Draw(look, side)), $"{sideName}-{EyeDoorLookNames.VisualName(look)}");
        }
        return keys;
    }
}
