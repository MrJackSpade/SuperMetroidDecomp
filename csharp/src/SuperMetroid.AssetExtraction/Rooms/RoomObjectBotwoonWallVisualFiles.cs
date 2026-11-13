using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

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
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Botwoon wall pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectBotwoonWallVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Assigns the lone wall-clear draw its stable artwork key and rejects other draw identities.</summary>
    internal static string VisualId(RoomObjectDraw draw) => draw == BotwoonWallObjectDrawDefinitions.Clear
        ? "clear-wall"
        : throw new InvalidDataException($"Botwoon wall draw {draw} has no visual ID.");

    /// <summary>
    /// The wall clear's draw list as the cartridge writes it: one run down the column from the wall's top block, each cell
    /// colliding as the game's block shape for the clear says (#1608). The run form is the cartridge's; the game draws
    /// the shape.
    /// </summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            RoomObjectDraw draw = BotwoonWallObjectDrawDefinitions.Clear;
            if (!BotwoonWallObjectDraws.TryDescribe(draw, out RoomBlockShape? shape))
                throw new InvalidOperationException("Botwoon's wall clear has no shape.");
            var collisions = new RoomCollisionType[shape.Cells.Count];
            for (int cell = 0; cell < collisions.Length; cell++) collisions[cell] = shape.CollisionAt(new Offset16<RoomBlocks>(0, cell));
            yield return RoomObjectDrawListExport.SingleRun(draw, RoomObjectRunDirection.Vertical, collisions);
        }
    }
}
