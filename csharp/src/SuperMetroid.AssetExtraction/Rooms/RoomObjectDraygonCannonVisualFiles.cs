using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;

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
    /// <param name="sourceCartridgeSha256">The cartridge's identity, recorded in the file.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the cannons draw by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, sourceCartridgeSha256, Family);

    /// <summary>Loads the installed cannon pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format or source is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledDraygonCannonFrames Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Every cannon frame's geometry, from the shapes the cannons draw by.</summary>
    private static Dictionary<DraygonCannonDraw, RoomObjectFrameLayout> Layouts()
    {
        var layouts = new Dictionary<DraygonCannonDraw, RoomObjectFrameLayout>();
        foreach (DraygonCannonDraw frame in Enum.GetValues<DraygonCannonDraw>())
        {
            RoomObjectDraw draw = RoomObjectDraw.Of(frame);
            if (!DraygonCannonDraws.TryDescribe(draw, out DraygonCannonDraws.Draw shape))
                throw new InvalidOperationException($"Draygon cannon frame {frame} has no shape.");
            layouts.Add(frame, RoomObjectFrameLayout.Of(draw, shape.RunCount, shape.BlockCount, shape.CollisionAt,
                RoomObjectDrawListExport.Horizontal, shape.Start));
        }
        return layouts;
    }
}
