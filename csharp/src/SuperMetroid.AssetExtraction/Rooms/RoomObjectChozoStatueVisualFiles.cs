using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed pictures of the three Chozo statue terrain frames (#1557), and the published names that key them.</summary>
public static class RoomObjectChozoStatueVisualFiles
{
    /// <summary>JSON filename for the cleared Chozo hand and Wrecked Ship slope-access appearances.</summary>
    public const string VisualFileName = "chozo-statues.json";

    /// <summary>The Chozo statue as the import pictures them: every frame's geometry and published name.</summary>
    internal static RoomObjectFrameFamily<RoomObjectDraw> Family { get; } =
        RoomObjectFrameImageFiles.DrawFamily(VisualFileName, "Chozo statue", All, VisualId);

    /// <summary>Pictures every Chozo statue frame from the cartridge's draw lists and writes the installed file (#1557).</summary>
    /// <param name="bus">The cartridge, whose draw lists are checked against the frames' geometry and collision.</param>
    /// <param name="directory">The installed directory, created if absent; the file must not already exist.</param>
    /// <param name="sourceCartridgeSha256">The cartridge's identity, recorded in the file.</param>
    /// <exception cref="InvalidDataException">A draw list differs from the geometry or collision the game draws by.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory, string sourceCartridgeSha256) =>
        RoomObjectFrameImageFiles.Extract(bus, directory, sourceCartridgeSha256, Family);

    /// <summary>Loads the installed Chozo statue pictures as the player left them, without the cartridge.</summary>
    /// <param name="directory">The installed directory.</param>
    /// <exception cref="InvalidDataException">The file's format or source is not this importer's, or a frame's picture is missing or misshapen.</exception>
    public static InstalledRoomObjectChozoStatueVisuals Load(string directory) =>
        new(RoomObjectFrameImageFiles.Load(directory, Family));

    /// <summary>Enumerates the three supported physical draw identities for artwork export.</summary>
    private static IEnumerable<RoomObjectDraw> Draws()
    {
        yield return RoomObjectDraw.Of(ChozoStatueDraw.LowerNorfairClearedHand);
        yield return RoomObjectDraw.Of(ChozoStatueDraw.ClearSlopeAccess);
        yield return RoomObjectDraw.Of(ChozoStatueDraw.BlockSlopeAccess);
    }

    /// <summary>The draw lists of the three statue draws.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (RoomObjectDraw draw in Draws())
            {
                TryGet(draw, out var list);
                yield return list;
            }
        }
    }

    /// <summary>
    /// The draw list of a statue frame as the cartridge writes it: its horizontal strips in write order, each cell
    /// colliding as the game's block shape for the frame says (#1622). The run form is the cartridge's; the game draws
    /// the shape.
    /// </summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        if (!ChozoStatueDraws.TryDescribe(request, out RoomBlockShape? described))
        {
            list = default;
            return false;
        }
        RoomBlockShape shape = described;
        // The cleared hand is written as its one block. Both slope-access frames are written as the ledge, the fill five
        // rows below it, and the step below the fill's sixth block, one row at a time.
        Offset16<RoomBlocks>[] handStarts = [new(0, 0)];
        int[] handCounts = [1];
        Offset16<RoomBlocks>[] accessStarts = [new(0, 0), new(0, 5), new(5, 6), new(5, 7), new(5, 8)];
        int[] accessCounts = [14, 9, 2, 1, 1];
        bool hand = RoomObjectDraw.FrameOf<ChozoStatueDraw>(request, "Chozo statue") == ChozoStatueDraw.LowerNorfairClearedHand;
        Offset16<RoomBlocks>[] starts = hand ? handStarts : accessStarts;
        int[] counts = hand ? handCounts : accessCounts;
        list = RoomObjectDrawListExport.Strips(request, starts.Length, run => counts[run],
            (run, cell) => shape.CollisionAt(starts[run] + new Offset16<RoomBlocks>(cell, 0)), run => starts[run]);
        return true;
    }

    /// <summary>Names the cleared hand or blocked/clear slope layout without changing collision ownership.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        RoomObjectVisualIdDefinitions.ChozoStatue.TryGetValue(
            RoomObjectDraw.FrameOf<ChozoStatueDraw>(request, "Chozo statue draw with a visual ID"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined {nameof(ChozoStatueDraw)} {request}.");
}
