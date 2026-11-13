using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Maridia elevatube PLM visual block, and the editable identity that names it.</summary>
public static class RoomObjectMaridiaElevatubeVisualFiles
{
    /// <summary>Family-relative installed JSON filename for the Maridia elevatube's single visual block, separate from Kraid's use of the same native draw.</summary>
    public const string VisualFileName = "maridia-elevatube.json";

    /// <summary>The Maridia elevatube as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Maridia elevatube", AllDraws, DrawVisualId);

    /// <summary>Pictures every Maridia elevatube frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Maridia elevatube pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectMaridiaElevatubeVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Stable asset identity assigned to the elevatube's one-block artwork.</summary>
    internal const string VisualId = "elevatube-block";

    /// <summary>The elevatube's one-block draw list, built from the block the game writes.</summary>
    internal static RoomObjectShotBlockDrawDefinitions.DrawList BlockDrawList =>
        RoomObjectDrawListExport.SingleRun(MaridiaElevatubeObjectDefinitions.BlockDraw, RoomObjectRunDirection.Horizontal,
            [MaridiaElevatubeObjectDefinitions.Collision]);

    /// <summary>The sole elevatube draw list.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> AllDraws => [BlockDrawList];

    /// <summary>Maps the elevatube draw to its asset identity and rejects unrelated draws.</summary>
    internal static string DrawVisualId(RoomObjectDraw request) => request == MaridiaElevatubeObjectDefinitions.BlockDraw
        ? VisualId
        : throw new InvalidDataException($"Maridia elevatube draw {request} has no visual ID.");
}
