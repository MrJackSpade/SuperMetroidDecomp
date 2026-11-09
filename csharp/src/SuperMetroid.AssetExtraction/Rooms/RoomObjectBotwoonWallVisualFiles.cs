using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Botwoon wall-clear visual blocks, and the editable identity that names them.</summary>
public static class RoomObjectBotwoonWallVisualFiles
{
    /// <summary>Editable Botwoon wall-clear visual JSON filename.</summary>
    public const string VisualFileName = "botwoon-wall.json";

    /// <summary>The Botwoon wall as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Botwoon wall", All, VisualId);

    /// <summary>Pictures every Botwoon wall frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <param name="sourceCartridgeSha256">The cartridge's identity, recorded in the file.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, sourceCartridgeSha256, Family);

    /// <summary>Loads the installed Botwoon wall pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format or source is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectBotwoonWallVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Assigns the lone wall-clear draw its stable artwork key and rejects other draw identities.</summary>
    internal static string VisualId(RoomObjectDraw draw) => draw == BotwoonWallObjectDrawDefinitions.Clear
        ? "clear-wall"
        : throw new InvalidDataException($"Botwoon wall draw {draw} has no visual ID.");

    /// <summary>The nine-block vertical clear draw, built from the blocks the game writes.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            var blocks = new RoomTerrainBlock[BotwoonWallObjectDrawDefinitions.BlockCount];
            for (int index = 0; index < blocks.Length; index++) blocks[index] = BotwoonWallObjectDraws.BlockAt(index);
            yield return RoomObjectDrawListExport.SingleRun(BotwoonWallObjectDrawDefinitions.Clear, RoomObjectRunDirection.Vertical, blocks);
        }
    }
}
