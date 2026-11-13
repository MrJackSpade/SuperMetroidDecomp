using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of Phantoon's door frames (#1678): the fight's door and the restored door frame.</summary>
public static class RoomObjectPhantoonDoorVisualFiles
{
    /// <summary>The installed file holding the door frames' pictures.</summary>
    public const string VisualFileName = "phantoon-door.json";

    /// <summary>The door as the import pictures it: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<PhantoonDoorDraw> Family { get; } =
        new(VisualFileName, "Phantoon door", Layouts(), RoomObjectVisualIdDefinitions.PhantoonDoor);

    /// <summary>Pictures the door frames from the cartridge's draw lists and writes the installed file.</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the door's geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the door draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed door pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledPhantoonDoorFrames Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>
    /// Every door frame's geometry as the cartridge writes it: one run down each column of the frame's block shape, the
    /// door's transition column first, each cell colliding as the shape says. The run form is the cartridge's; the game
    /// draws the shape.
    /// </summary>
    private static Dictionary<PhantoonDoorDraw, RoomObjectFrameLayout> Layouts()
    {
        var layouts = new Dictionary<PhantoonDoorDraw, RoomObjectFrameLayout>();
        foreach (PhantoonDoorDraw frame in Enum.GetValues<PhantoonDoorDraw>())
        {
            RoomObjectDraw draw = RoomObjectDraw.Of(frame);
            if (!PhantoonDoorDraws.TryDescribe(draw, out RoomBlockShape? described))
                throw new InvalidOperationException($"Phantoon door frame {frame} has no shape.");
            RoomBlockShape shape = described;
            int[] columns = [.. shape.Cells.Select(cell => cell.Cell.X).Distinct().Order()];
            int Height(int column) => shape.Cells.Count(cell => cell.Cell.X == columns[column]);
            layouts.Add(frame, RoomObjectFrameLayout.Of(draw, columns.Length, Height,
                (run, cell) => shape.CollisionAt(new Offset16<RoomBlocks>(columns[run], cell)),
                _ => RoomObjectRunDirection.Vertical, run => new Offset16<RoomBlocks>(columns[run], 0)));
        }
        return layouts;
    }
}
