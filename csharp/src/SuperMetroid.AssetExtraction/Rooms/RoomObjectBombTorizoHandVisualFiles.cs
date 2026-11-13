using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of Bomb Torizo's intact and cleared hand frames (#1557), and the published names that key them.</summary>
public static class RoomObjectBombTorizoHandVisualFiles
{
    /// <summary>Editable Bomb Torizo hand PLM visual JSON filename.</summary>
    public const string VisualFileName = "bomb-torizo-hand.json";

    /// <summary>The Bomb Torizo hand as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Bomb Torizo hand", All, VisualId);

    /// <summary>Pictures every Bomb Torizo hand frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, Family);

    /// <summary>Loads the installed Bomb Torizo hand pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectBombTorizoHandVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>The intact and cleared hand draw lists, in authored order.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            TryGet(RoomObjectDraw.Of(BombTorizoHandDraw.Intact), out var intact);
            yield return intact;
            TryGet(RoomObjectDraw.Of(BombTorizoHandDraw.Cleared), out var cleared);
            yield return cleared;
        }
    }

    /// <summary>
    /// The draw list of the intact or cleared hand as the cartridge writes it: its horizontal strips in write order, each
    /// cell colliding as the game's block shape for the frame says (#1608). The run form is the cartridge's; the game
    /// draws the shape.
    /// </summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        if (!BombTorizoHandDraws.TryDescribe(request, out RoomBlockShape? described))
        {
            list = default;
            return false;
        }
        RoomBlockShape shape = described;
        // The intact hand is written as the hand's block and the one right of it, the block left of it, the pair above
        // and the three below. The cleared hand splits the hand's row into its right and left halves, then writes the
        // rows below, at the top and above.
        Offset16<RoomBlocks>[] intactStarts = [new(0, 0), new(-1, 0), new(0, -1), new(-1, 1)];
        int[] intactCounts = [2, 1, 2, 3];
        Offset16<RoomBlocks>[] clearedStarts = [new(0, 0), new(-2, 0), new(-2, 1), new(-2, -2), new(-2, -1)];
        int[] clearedCounts = [2, 2, 4, 4, 4];
        bool intact = RoomObjectDraw.FrameOf<BombTorizoHandDraw>(request, "Bomb Torizo hand") == BombTorizoHandDraw.Intact;
        Offset16<RoomBlocks>[] starts = intact ? intactStarts : clearedStarts;
        int[] counts = intact ? intactCounts : clearedCounts;
        list = RoomObjectDrawListExport.Strips(request, starts.Length, run => counts[run],
            (run, cell) => shape.CollisionAt(starts[run] + new Offset16<RoomBlocks>(cell, 0)), run => starts[run]);
        return true;
    }

    /// <summary>Gets the stable artwork ID for a supported hand draw.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        VisualId(RoomObjectDraw.FrameOf<BombTorizoHandDraw>(request, "Bomb Torizo hand draw with a visual ID"));

    /// <summary>Maps an intact or cleared hand draw to its stable artwork ID.</summary>
    internal static string VisualId(BombTorizoHandDraw frame) =>
        RoomObjectVisualIdDefinitions.BombTorizoHand.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined BombTorizoHandDraw {frame}.");
}
