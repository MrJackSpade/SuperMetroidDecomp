using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Areas.Tourian.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Tourian entrance access-floor visuals.</summary>
public static class RoomObjectTourianAccessVisualFiles
{
    /// <summary>Editable Tourian entrance access-floor visual JSON filename.</summary>
    public const string VisualFileName = "tourian-access.json";

    /// <summary>The Tourian access floor as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Tourian access", All, VisualId);

    /// <summary>Pictures every Tourian access floor frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Tourian access floor pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static RoomObjectTourianAccessVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The draw lists of every Tourian access draw.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (TourianAccessDraw draw in Enum.GetValues<TourianAccessDraw>())
                yield return Compile(draw);
        }
    }

    /// <summary>Gets the stable artwork identity of a Tourian access draw.</summary>
    internal static string VisualId(RoomObjectDraw draw) => VisualId(RoomObjectDraw.FrameOf<TourianAccessDraw>(draw, "Tourian access"));

    /// <summary>Names the empty, three crumble, or six-row clear frame for visual files.</summary>
    internal static string VisualId(TourianAccessDraw draw) =>
        RoomObjectVisualIdDefinitions.TourianAccess.TryGetValue(draw, out string? id)
            ? id : throw new InvalidOperationException($"Undefined TourianAccessDraw {draw}.");

    /// <summary>Builds the four-air-cell runs of a Tourian access draw.</summary>
    internal static RoomObjectShotBlockDrawDefinitions.DrawList Compile(TourianAccessDraw draw) =>
        RoomObjectDrawListExport.Runs(RoomObjectDraw.Of(draw), TourianAccessDraws.Rows(draw), static _ => TourianAccessObjectDrawDefinitions.RowWidth,
            static (_, _) => TourianAccessObjectDrawDefinitions.Collision, RoomObjectDrawListExport.Horizontal, RoomObjectDrawListExport.Stacked);
}
