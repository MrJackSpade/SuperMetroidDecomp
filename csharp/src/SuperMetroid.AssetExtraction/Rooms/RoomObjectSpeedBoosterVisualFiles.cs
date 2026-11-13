using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Speed Booster bomb-reveal visual block, and the editable identity that names it.</summary>
public static class RoomObjectSpeedBoosterVisualFiles
{
    /// <summary>Editable Speed Booster bomb-reveal visual JSON filename.</summary>
    public const string VisualFileName = "speed-booster.json";

    /// <summary>The Speed Booster reveal as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Speed Booster", All, VisualId);

    /// <summary>Pictures every Speed Booster reveal frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Speed Booster reveal pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectSpeedBoosterVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The artwork identity of the bomb reveal of one Speed Booster block.</summary>
    internal const string BombRevealVisualId = "bomb-reveal";

    /// <summary>The one-block reveal's draw list, built from the block the game writes.</summary>
    internal static RoomObjectShotBlockDrawDefinitions.DrawList BombReveal =>
        RoomObjectDrawListExport.SingleRun(SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw, RoomObjectRunDirection.Horizontal,
            [SpeedBoosterBlockObjectDrawDefinitions.BombRevealCollision]);

    /// <summary>The sole bomb-reveal draw list.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All => [BombReveal];

    /// <summary>Accepts only the bomb-reveal draw and rejects unrelated PLM drawings.</summary>
    internal static string VisualId(RoomObjectDraw request) => request == SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw
        ? BombRevealVisualId
        : throw new InvalidDataException($"Speed Booster draw {request} has no visual ID.");
}
