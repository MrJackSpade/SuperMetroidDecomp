using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of Spore Spawn's four ceiling frames (#1557), and the published names that key them.</summary>
public static class RoomObjectSporeSpawnCeilingVisualFiles
{
    /// <summary>Editable Spore Spawn ceiling visual JSON filename.</summary>
    private const string VisualFileName = "spore-spawn-ceiling.json";

    /// <summary>The ceiling's frames in the installed file's order (#2567): the clear, then the ceiling cracked, crumbling and crumbled.</summary>
    private static readonly IReadOnlyList<SporeSpawnCeilingDraw> FrameOrder = Ensure.EveryMemberOnce<SporeSpawnCeilingDraw>(
    [
        SporeSpawnCeilingDraw.Clear,
        SporeSpawnCeilingDraw.CeilingCracked,
        SporeSpawnCeilingDraw.CeilingCrumbling,
        SporeSpawnCeilingDraw.CeilingCrumbled,
    ]);

    /// <summary>The Spore Spawn ceiling as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Spore Spawn ceiling", All, VisualId);

    /// <summary>Pictures every Spore Spawn ceiling frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Spore Spawn ceiling pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectPictures Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The draw lists of the clear and crumble frames.</summary>
    private static IEnumerable<RoomObjectDrawLists.DrawList> All
    {
        get
        {
            foreach (SporeSpawnCeilingDraw frame in FrameOrder)
            {
                yield return DrawListOf(SporeSpawnCeilingDraws.Family.Of(frame));
            }
        }
    }

    /// <summary>Selects a ceiling draw's published artwork key.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        VisualId(RoomObjectDrawIdentity.FrameOf<SporeSpawnCeilingDraw>(request, "Spore Spawn ceiling draw with a visual ID"));

    /// <summary>Names the clear layout and the three successive crumble appearances for visual editing.</summary>
    internal static string VisualId(SporeSpawnCeilingDraw frame) => SporeSpawnCeilingDrawKeys.Installed.Of(frame);

    /// <summary>The draw list of <paramref name="draw"/>, which must be one of the Spore Spawn ceiling draws.</summary>
    /// <exception cref="InvalidDataException"><paramref name="draw"/> is not a Spore Spawn ceiling draw.</exception>
    internal static RoomObjectDrawLists.DrawList DrawListOf(RoomObjectDraw draw) =>
        TryGet(draw, out RoomObjectDrawLists.DrawList list) ? list : throw new InvalidDataException($"{draw} is not a Spore Spawn ceiling draw.");

    /// <summary>
    /// The draw list of a clear or crumble frame as the cartridge writes it: the square's two rows, top row first, each
    /// cell colliding as the game's block shape for the frame says (#1608). The run form is the cartridge's; the game
    /// draws the shape.
    /// </summary>
    private static bool TryGet(RoomObjectDraw request, out RoomObjectDrawLists.DrawList list)
    {
        if (!RoomObjectDrawIdentity.TryShape(SporeSpawnCeilingDraws.Family, request, out RoomBlockShape? described))
        {
            list = default;
            return false;
        }
        int rowWidth = 2;
        list = RoomObjectDrawListExport.Runs(request, described,
            [RoomObjectShapeRun.Row(new(0, 0), rowWidth), RoomObjectShapeRun.Row(new(0, 1), rowWidth)]);
        return true;
    }
}
