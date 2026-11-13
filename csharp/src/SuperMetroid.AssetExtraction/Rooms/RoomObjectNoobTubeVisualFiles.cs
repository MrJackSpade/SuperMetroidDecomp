using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge-checked, strictly visual-only installed n00b-tube artwork.</summary>
public static class RoomObjectNoobTubeVisualFiles
{
    /// <summary>Editable n00b-tube visual JSON filename.</summary>
    public const string VisualFileName = "noob-tube.json";

    /// <summary>The n00b-tube as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "n00b-tube", All, VisualId);

    /// <summary>Pictures every n00b-tube frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed n00b-tube pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectNoobTubeVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Enumerates tube states in intact-to-cleared and broken presentation order.</summary>
    private static IEnumerable<NoobTubeDraw> Draws()
    {
        yield return NoobTubeDraw.Intact;
        yield return NoobTubeDraw.Damaged;
        yield return NoobTubeDraw.Opened;
        yield return NoobTubeDraw.Cleared;
        yield return NoobTubeDraw.BrokenLate;
        yield return NoobTubeDraw.OpenedRows;
        yield return NoobTubeDraw.BrokenFull;
    }

    /// <summary>Builds the draw list of a tube layout from its computed cells.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        bool found = NoobTubeObjectDraws.TryDescribe(request, out NoobTubeObjectDraws.Draw draw);
        list = found
            ? RoomObjectDrawListExport.Strips(request, draw.RunCount, draw.BlockCount, draw.CollisionAt, draw.Start)

            : default;
        return found;
    }

    /// <summary>Converts a validated tube draw to its artwork catalog ID.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        VisualId(RoomObjectDraw.FrameOf<NoobTubeDraw>(request, "n00b-tube draw with a visual ID"));

    /// <summary>Names the typed intact, damaged, opened, cleared, or broken presentation state.</summary>
    internal static string VisualId(NoobTubeDraw frame) => frame switch
    {
        NoobTubeDraw.Intact => "intact",
        NoobTubeDraw.Damaged => "damaged",
        NoobTubeDraw.Opened => "opened",
        NoobTubeDraw.Cleared => "cleared",
        NoobTubeDraw.BrokenLate => "broken-late",
        NoobTubeDraw.OpenedRows => "opened-rows",
        NoobTubeDraw.BrokenFull => "broken-full",
        _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {frame}."),
    };

    /// <summary>Builds each known tube layout's draw list.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (NoobTubeDraw frame in Draws())
            {
                TryGet(RoomObjectDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }
}
