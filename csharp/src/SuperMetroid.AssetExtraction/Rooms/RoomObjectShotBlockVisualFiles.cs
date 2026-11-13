using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the nineteen ordinary shot-block frames (#1557), and the published names that key them.</summary>
public static class RoomObjectShotBlockVisualFiles
{
    /// <summary>Family-relative installed JSON filename for all nineteen ordinary shot-block breakup and restoration draws.</summary>
    public const string VisualFileName = "shot-blocks.json";
    /// <summary>The shot-block as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Shot-block", All, VisualId);

    /// <summary>Pictures every shot-block frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed shot-block pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectShotBlockVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The published artwork identity of a shot-block draw: its shape and breakup frame, or its restoration.</summary>
    internal static string VisualId(ShotBlockObjectDraws.Draw draw)
    {
        string shape = draw.Layout switch
        {
            RoomObjectShotBlockDrawDefinitions.Shape.Single => "single",
            RoomObjectShotBlockDrawDefinitions.Shape.Horizontal => "horizontal",
            RoomObjectShotBlockDrawDefinitions.Shape.Vertical => "vertical",
            RoomObjectShotBlockDrawDefinitions.Shape.Square => "square",
            _ => throw new InvalidOperationException($"Undefined shot-block shape {draw.Layout}."),
        };
        return draw.Restore ? $"{shape}-restore" : $"{shape}-breakup-{draw.Frame}";
    }

    /// <summary>The published artwork identity of a shot-block <paramref name="draw"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a shot-block draw.</exception>
    internal static string VisualId(RoomObjectDraw draw) => VisualId(RoomObjectDraw.FrameOf<ShotBlockObjectDraws.Draw>(draw, "shot-block"));

    /// <summary>The draw lists of every breakup and restoration draw, built from the calculated draws.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All => Calculated.Select(Export);

    /// <summary>Expands a calculated shot-block draw into ordered runs of collision and their offsets.</summary>
    private static RoomObjectShotBlockDrawDefinitions.DrawList Export(ShotBlockObjectDraws.Draw draw) =>
        RoomObjectDrawListExport.Runs(RoomObjectDraw.Of(draw), draw.RunCount, _ => draw.BlocksPerRun, draw.CollisionAt,
            _ => draw.Vertical ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal,
            RoomObjectDrawListExport.Stacked);

    /// <summary>Yields breakup frames interleaved by shape, followed by the three restoration draws.</summary>
    internal static IEnumerable<ShotBlockObjectDraws.Draw> Calculated
    {
        get
        {
            for (int frame = 0; frame < RoomObjectShotBlockDrawDefinitions.BreakupFrameCount; frame++)
            {
                yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Single, frame, Restore: false);
                yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Horizontal, frame, Restore: false);
                yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Vertical, frame, Restore: false);
                yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Square, frame, Restore: false);
            }
            yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Horizontal, 0, Restore: true);
            yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Vertical, 0, Restore: true);
            yield return new(RoomObjectShotBlockDrawDefinitions.Shape.Square, 0, Restore: true);
        }
    }
}
