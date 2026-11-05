using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable Spore Spawn ceiling visual blocks.</summary>
public static class RoomPlmSporeSpawnCeilingVisualFiles
{
    /// <summary>Editable Spore Spawn ceiling visual JSON filename.</summary>
    public const string VisualFileName = "spore-spawn-ceiling.json";

    /// <summary>Verifies every native Spore Spawn ceiling draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Spore Spawn ceiling",
            All,
            VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable Spore Spawn ceiling override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and Spore Spawn ceiling JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose Spore Spawn ceiling JSON is selected when present.</param>
    /// <returns>The immutable selected Spore Spawn ceiling visual catalog.</returns>
    public static RoomPlmSporeSpawnCeilingVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            new RoomPlmVisualFamily<RoomPlmSporeSpawnCeilingVisualCatalog>(VisualFileName, "Spore Spawn ceiling", entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates complete coverage of the clear and three crumble frames and builds the visuals from the appearances that differ from the compiled frames, without changing compiled geometry or collision.</summary>
    /// <param name="entries">Each of the four compiled visual identities exactly once, with four appearances per frame.</param>
    /// <remarks>Edited arrays are copied; stock-identical frames use compiled appearances, so later edits to caller arrays cannot change this catalog.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">A frame is missing, duplicated, null, unknown, or incorrectly shaped.</exception>
    internal static RoomPlmSporeSpawnCeilingVisualCatalog Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Spore Spawn ceiling", TryGetByVisualId, All.Count()));

    /// <summary>The draw lists of the clear and crumble frames.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (SporeSpawnCeilingDraw frame in Enum.GetValues<SporeSpawnCeilingDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Selects a ceiling draw's published artwork key.</summary>
    internal static string VisualId(PlmDraw request) =>
        VisualId(PlmDraw.FrameOf<SporeSpawnCeilingDraw>(request, "Spore Spawn ceiling draw with a visual ID"));

    /// <summary>Names the clear layout and the three successive crumble appearances for visual editing.</summary>
    internal static string VisualId(SporeSpawnCeilingDraw frame) =>
        RoomPlmVisualIdDefinitions.SporeSpawnCeiling.TryGetValue(frame, out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined SporeSpawnCeilingDraw {frame}.");

    /// <summary>Finds the physical square associated with an editable ceiling-frame key.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Builds the two-run square of a supported clear or crumble draw.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (SporeSpawnCeilingDraws.TryGetBlock(request, out RoomTerrainBlock block))
        {
            list = Square(request, block);
            return true;
        }
        list = default;
        return false;
    }

    /// <summary>Duplicates one terrain block across the two horizontal runs of the ceiling's two-by-two layout.</summary>
    private static RoomPlmShotBlockDrawDefinitions.DrawList Square(PlmDraw draw, RoomTerrainBlock block) =>
        RoomPlmDrawListExport.Runs(draw, 2, static _ => 2, (_, _) => block,
            RoomPlmDrawListExport.Horizontal, static _ => 0, static run => run == 0 ? 1 : 0);
}
