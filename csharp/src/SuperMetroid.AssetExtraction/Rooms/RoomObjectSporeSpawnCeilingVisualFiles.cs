using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of Spore Spawn's four ceiling frames (#1557), and the published names that key them.</summary>
public static class RoomObjectSporeSpawnCeilingVisualFiles
{
    /// <summary>Editable Spore Spawn ceiling visual JSON filename.</summary>
    public const string VisualFileName = "spore-spawn-ceiling.json";

    /// <summary>The Spore Spawn ceiling as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Spore Spawn ceiling", All, VisualId);

    /// <summary>Pictures every Spore Spawn ceiling frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <param name="sourceCartridgeSha256">The cartridge's identity, recorded in the file.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, sourceCartridgeSha256, Family);

    /// <summary>Loads the installed Spore Spawn ceiling pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format or source is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectSporeSpawnCeilingVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The draw lists of the clear and crumble frames.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (SporeSpawnCeilingDraw frame in Enum.GetValues<SporeSpawnCeilingDraw>())
            {
                TryGet(RoomObjectDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Selects a ceiling draw's published artwork key.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        VisualId(RoomObjectDraw.FrameOf<SporeSpawnCeilingDraw>(request, "Spore Spawn ceiling draw with a visual ID"));

    /// <summary>Names the clear layout and the three successive crumble appearances for visual editing.</summary>
    internal static string VisualId(SporeSpawnCeilingDraw frame) =>
        RoomObjectVisualIdDefinitions.SporeSpawnCeiling.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {frame}.");

    /// <summary>Builds the two-run square of a supported clear or crumble draw.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        if (SporeSpawnCeilingDraws.TryGetCollision(request, out RoomCollisionType collision))
        {
            list = Square(request, collision);
            return true;
        }
        list = default;
        return false;
    }

    /// <summary>Repeats one collision kind across the two horizontal runs of the ceiling's two-by-two layout.</summary>
    private static RoomObjectShotBlockDrawDefinitions.DrawList Square(RoomObjectDraw draw, RoomCollisionType collision) =>
        RoomObjectDrawListExport.Runs(draw, 2, static _ => 2, (_, _) => collision,
            RoomObjectDrawListExport.Horizontal, RoomObjectDrawListExport.Stacked);
}
