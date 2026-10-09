using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Cartridge-checked stock and strict visual-only glass overrides.</summary>
public static class RoomPlmMotherBrainGlassVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for Mother Brain's initial, damaged, shifted, shattering, and cleared glass PLM layouts.</summary>
    public const string VisualFileName = "mother-brain-glass.json";

    /// <summary>Checks native bank-$84 glass draw shapes, physical words, and offsets, then exports their appearance-only words in flattened run order.</summary>
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
            VisualFileName, "Mother Brain glass", MotherBrainGlassPlmDrawDefinitions.All,
            MotherBrainGlassPlmDrawDefinitions.VisualId);

    /// <summary>Validates installed stock glass appearance and selects an optional complete visual replacement while preserving glass mechanics.</summary>
    /// <param name="stockDirectory">Family directory containing the required stock JSON and version-one manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or a missing override JSON selects validated stock.</param>
    /// <returns>A ROM-independent glass metatile catalog; draw geometry, hit thresholds, shards, and event timing remain compiled.</returns>
    /// <remarks>Supported-cartridge provenance, byte hash, and compiled stock visual words are always checked first. Overrides need all compiled identities, original run/cell counts, and twelve-bit visual words, but no manifest.</remarks>
    /// <exception cref="InvalidDataException">Integrity, provenance, JSON version, frame coverage/identity, shape, or presentation-only word bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmMotherBrainGlassVisualCatalog Load(string stockDirectory,
        string? overrideDirectory = null) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Mother Brain glass", MotherBrainGlassPlmDrawDefinitions.All,
            MotherBrainGlassPlmDrawDefinitions.VisualId,
            entries => new RoomPlmMotherBrainGlassVisualCatalog(entries.Select(entry =>
                new RoomPlmMotherBrainGlassVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Runs stock glass manifest, integrity, schema, and compiled-appearance checks without overrides, cartridge reads, or file writes.</summary>
    /// <param name="stockDirectory">Glass family directory containing both required stock files.</param>
    /// <exception cref="InvalidDataException">The stock installation fails the checks performed by <see cref="Load"/>.</exception>
    public static void ValidateStock(string stockDirectory) => Load(stockDirectory);
}
