using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of Draygon's wall cannons' twelve frames (#1557).</summary>
public static class RoomObjectDraygonCannonVisualFiles
{
    /// <summary>The installed file holding the right- and left-facing cannon frames' pictures.</summary>
    public const string VisualFileName = "draygon-cannons.json";

    /// <summary>The cannons as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<DraygonCannonDraw> Family { get; } =
        new(VisualFileName, "Draygon cannon", Layouts(), RoomObjectVisualIdDefinitions.DraygonCannon);

    /// <summary>Pictures the twelve cannon frames from the cartridge's draw lists and writes the installed file.</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the cannons' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the cannons draw by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed cannon pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledDraygonCannonFrames Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>
    /// Every cannon frame's geometry as the cartridge writes it: the runs of its draw list, each cell colliding as the
    /// game's block shape for the frame says (#1608). The run form is the cartridge's; the game draws the shape.
    /// </summary>
    private static Dictionary<DraygonCannonDraw, RoomObjectFrameLayout> Layouts()
    {
        // A frame reaching right of the cannon's core is written as its two rows, core row first. A frame reaching left
        // splits its top row into the core and the cell to its left, then writes the lower row from its left end.
        Offset16<RoomBlocks> rightOfCore = new(1, 0);
        Offset16<RoomBlocks>[] rightStarts = [new(0, 0), new(0, 1)];
        int[] rightCounts = [2, 2];
        Offset16<RoomBlocks>[] leftStarts = [new(0, 0), new(-1, 0), new(-1, 1)];
        int[] leftCounts = [1, 1, 2];

        var layouts = new Dictionary<DraygonCannonDraw, RoomObjectFrameLayout>();
        foreach (DraygonCannonDraw frame in Enum.GetValues<DraygonCannonDraw>())
        {
            RoomObjectDraw draw = RoomObjectDraw.Of(frame);
            if (!DraygonCannonDraws.TryDescribe(draw, out RoomBlockShape? described))
                throw new InvalidOperationException($"Draygon cannon frame {frame} has no shape.");
            RoomBlockShape shape = described;
            bool right = shape.Covers(rightOfCore);
            Offset16<RoomBlocks>[] starts = right ? rightStarts : leftStarts;
            int[] counts = right ? rightCounts : leftCounts;
            layouts.Add(frame, RoomObjectFrameLayout.Of(draw, starts.Length, run => counts[run],
                (run, cell) => shape.CollisionAt(starts[run] + new Offset16<RoomBlocks>(cell, 0)),
                RoomObjectDrawListExport.Horizontal, run => starts[run]));
        }
        return layouts;
    }
}
