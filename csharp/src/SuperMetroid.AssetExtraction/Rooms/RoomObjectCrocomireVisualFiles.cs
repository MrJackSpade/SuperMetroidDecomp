using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of Crocomire's five bridge and invisible-wall frames (#1557), and the published names that key them.</summary>
public static class RoomObjectCrocomireVisualFiles
{
    /// <summary>The run layout the exported draw lists record for a bridge or wall draw.</summary>
    extension(CrocomireArenaDraws.Draw shape)
    {
        /// <summary>Wall columns write down their column; the bridge writes along its row.</summary>
        internal RoomObjectRunDirection Direction => shape.Wall ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal;

        /// <summary>Where a run starts: each wall column its run number of blocks right of the draw origin.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="run"/> is not one of the draw's runs.</exception>
        internal Offset16<RoomBlocks> Start(int run)
        {
            if ((uint)run >= shape.RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return new(run, 0);
        }
    }

    /// <summary>JSON filename for Crocomire bridge and invisible-wall visual layouts.</summary>
    public const string VisualFileName = "crocomire-arena.json";

    /// <summary>The Crocomire arena as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Crocomire arena", All, VisualId);

    /// <summary>Pictures every Crocomire arena frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <param name="sourceCartridgeSha256">The cartridge's identity, recorded in the file.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, sourceCartridgeSha256, Family);

    /// <summary>Loads the installed Crocomire arena pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format or source is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectCrocomireVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The draw lists of every bridge and wall draw.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (CrocomireArenaDraw frame in Enum.GetValues<CrocomireArenaDraw>())
            {
                TryGet(RoomObjectDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Builds the draw list of one bridge or wall draw with its run offsets.</summary>
    internal static bool TryGet(RoomObjectDraw draw, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        bool found = CrocomireArenaDraws.TryDescribe(draw, out CrocomireArenaDraws.Draw shape);
        list = found
            ? RoomObjectDrawListExport.Runs(draw, shape.RunCount, _ => shape.BlocksPerRun, shape.CollisionAt,

                _ => shape.Direction, run => shape.Start(run))
            : default;
        return found;
    }

    /// <summary>Maps an arena draw to the published bridge or wall artwork key.</summary>
    internal static string VisualId(RoomObjectDraw draw) =>
        RoomObjectVisualIdDefinitions.CrocomireArena[RoomObjectDraw.FrameOf<CrocomireArenaDraw>(draw, "Crocomire arena")];
}
