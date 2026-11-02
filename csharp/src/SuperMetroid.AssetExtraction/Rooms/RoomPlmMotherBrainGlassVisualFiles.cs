using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge-checked stock and strict visual-only glass overrides.</summary>
public static class RoomPlmMotherBrainGlassVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for Mother Brain's initial, damaged, shifted, shattering, and cleared glass PLM layouts.</summary>
    public const string VisualFileName = "mother-brain-glass.json";

    /// <summary>Checks native bank-$84 glass draw shapes, decoded terrain blocks, and offsets, then exports their block appearances in flattened run order.</summary>
    /// <param name="bus">Cartridge source for the compiled glass layouts; this extraction does not export shard sprites, hit thresholds, or event timing.</param>
    /// <param name="directory">Family directory, created if absent; receives new <see cref="VisualFileName"/> and <c>manifest.json</c> files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, recorded without recomputing it; a stock load requires the supported cartridge identity.</param>
    /// <remarks>Existing outputs are not overwritten. A failure writing the second file can leave the first file behind.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native draw data does not match the compiled glass layouts.</exception>
    /// <exception cref="IOException">An output already exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Mother Brain glass", All,
            VisualId);

    /// <summary>Validates installed stock glass appearance and selects an optional complete visual replacement while preserving glass mechanics.</summary>
    /// <param name="stockDirectory">Family directory containing the required stock JSON and version-one manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or a missing override JSON selects validated stock.</param>
    /// <returns>A ROM-independent glass metatile catalog; draw geometry, hit thresholds, shards, and event timing remain compiled.</returns>
    /// <remarks>Supported-cartridge provenance, byte hash, and compiled stock appearances are always checked first. Overrides need all compiled identities, original run/cell counts, and appearances, but no manifest.</remarks>
    /// <exception cref="InvalidDataException">Integrity, provenance, JSON version, frame coverage/identity, shape, or presentation-only word bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmMotherBrainGlassVisualCatalog Load(string stockDirectory,
        string? overrideDirectory = null) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            new RoomPlmVisualFamily<RoomPlmMotherBrainGlassVisualCatalog>(VisualFileName, "Mother Brain glass", All, VisualId, entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks))), (catalog, draw, run, block) => catalog.GetAppearance(draw, run, block)));

    /// <summary>Runs stock glass manifest, integrity, schema, and compiled-appearance checks without overrides, cartridge reads, or file writes.</summary>
    /// <param name="stockDirectory">Glass family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">The stock installation fails the checks performed by <see cref="Load"/>.</exception>
    public static void ValidateStock(string stockDirectory) => Load(stockDirectory);

    /// <summary>Validates all eleven glass layouts and builds the visuals from the appearances that differ from the compiled layouts, without changing damage thresholds, shards, event timing, terrain blocks, or signed draw offsets.</summary>
    /// <param name="entries">Exactly one visual-only entry per compiled glass identity, with the full flattened count of its run cells.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves frame coverage incomplete.</exception>
    internal static RoomPlmMotherBrainGlassVisualCatalog Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Mother Brain glass", TryGetByVisualId, All.Count()));

    /// <summary>Builds the eleven glass layouts' draw lists in order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (MotherBrainGlassDraw frame in Enum.GetValues<MotherBrainGlassDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Builds the draw list of a glass layout from its cells.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        bool found = MotherBrainGlassDraws.TryDescribe(request, out MotherBrainGlassDraws.Draw draw);
        list = found
            ? RoomPlmDrawListExport.Runs(request, draw.RunCount, draw.BlockCount, draw.BlockAt,
                run => draw.Vertical(run) ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal, draw.NextX, draw.NextY)
            : default;
        return found;
    }

    /// <summary>Finds a glass layout by its stable artwork identifier.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (MotherBrainGlassDraw frame in Enum.GetValues<MotherBrainGlassDraw>())
            if (string.Equals(id, VisualId(PlmDraw.Of(frame)), StringComparison.Ordinal)) return TryGet(PlmDraw.Of(frame), out list);
        list = default;
        return false;
    }

    /// <summary>Maps a glass draw to its editable presentation frame name.</summary>
    internal static string VisualId(PlmDraw request) =>
        RoomPlmVisualIdDefinitions.MotherBrainGlass.TryGetValue(
            PlmDraw.FrameOf<MotherBrainGlassDraw>(request, "Mother Brain glass draw"), out string? id)
            ? id
            : throw new InvalidOperationException($"Undefined Mother Brain glass draw {request}.");
}
