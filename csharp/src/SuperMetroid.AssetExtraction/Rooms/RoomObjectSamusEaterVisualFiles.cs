using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Extracts and loads the eight replaceable Samus Eater block frames.</summary>
public static class RoomObjectSamusEaterVisualFiles
{
    /// <summary>Editable Samus Eater plant visual JSON filename.</summary>
    public const string VisualFileName = "samus-eater.json";

    /// <summary>The Samus Eater plant as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Samus Eater plant", All, VisualId);

    /// <summary>Pictures every Samus Eater plant frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Samus Eater plant pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectSamusEaterVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Maps a validated draw to the artwork catalog's orientation and phase ID.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        VisualId(RoomObjectDraw.FrameOf<SamusEaterDraw>(request, "Samus Eater draw with a visual ID"));

    /// <summary>Names a typed draw list by its floor or ceiling orientation and idle or chew phase.</summary>
    internal static string VisualId(SamusEaterDraw frame) => frame switch
    {
        SamusEaterDraw.FloorIdle => "floor-idle",
        SamusEaterDraw.FloorChew1 => "floor-chew-1",
        SamusEaterDraw.FloorChew2 => "floor-chew-2",
        SamusEaterDraw.FloorChew3 => "floor-chew-3",
        SamusEaterDraw.CeilingIdle => "ceiling-idle",
        SamusEaterDraw.CeilingChew1 => "ceiling-chew-1",
        SamusEaterDraw.CeilingChew2 => "ceiling-chew-2",
        SamusEaterDraw.CeilingChew3 => "ceiling-chew-3",
        _ => throw new InvalidOperationException($"Undefined SamusEaterDraw {frame}."),
    };

    /// <summary>
    /// The draw list of a plant draw (#1862): its block shape written as the right half of the mouth, its left half two
    /// blocks left, and the four-wide foliage below those for a floor plant or above them for a ceiling one.
    /// </summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        if (!SamusEaterObjectDraws.TryDescribe(request, out RoomBlockShape? shape))
        {
            list = default;
            return false;
        }
        bool ceiling = RoomObjectDraw.FrameOf<SamusEaterDraw>(request, "Samus Eater") is SamusEaterDraw.CeilingIdle
            or SamusEaterDraw.CeilingChew1 or SamusEaterDraw.CeilingChew2 or SamusEaterDraw.CeilingChew3;
        list = RoomObjectShapeRun.DrawList(request, shape, RoomObjectShapeRun.Row(new(0, 0), 2), RoomObjectShapeRun.Row(new(-2, 0), 2),
            RoomObjectShapeRun.Row(new(-2, ceiling ? 1 : -1), 4));
        return true;
    }

    /// <summary>Builds the eight plant layouts' draw lists.</summary>
    internal static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            foreach (SamusEaterDraw frame in Enum.GetValues<SamusEaterDraw>())
            {
                TryGet(RoomObjectDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }
}
