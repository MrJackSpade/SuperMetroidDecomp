using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Mother Brain fake-death room tile art.</summary>
public static class RoomPlmMotherBrainFakeDeathVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for Mother Brain's fake-death room walls, escape door, background rows, and tube/ceiling clears.</summary>
    public const string VisualFileName = "mother-brain-fake-death.json";

    /// <summary>Verifies the bank-$84 fake-death room draw layouts and exports their flattened visual words, leaving physical room mutations and encounter timing compiled.</summary>
    /// <param name="bus">Cartridge source for draws at $84:94A3–9716, including the cataloged unused background rows E and F.</param>
    /// <param name="directory">Family directory, created if absent; receives new <see cref="VisualFileName"/> and <c>manifest.json</c> files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied cartridge provenance hash; stock loading requires the supported identity rather than trusting arbitrary provenance.</param>
    /// <remarks>Only metatile/flip bits are serialized in native run/cell order. Create-new writes refuse existing outputs and are not an atomic JSON/manifest transaction.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native run shapes, physical words, or offsets differ from compiled definitions.</exception>
    /// <exception cref="IOException">An output already exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Mother Brain fake-death room",
            MotherBrainFakeDeathPlmDrawDefinitions.All,
            MotherBrainFakeDeathPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock fake-death room artwork before selecting a complete optional replacement of its visual layouts.</summary>
    /// <param name="stockDirectory">Family directory containing the required version-one manifest and stock JSON.</param>
    /// <param name="overrideDirectory">Optional family directory; null or absent override JSON uses validated stock.</param>
    /// <returns>A ROM-independent room-metatile catalog, not Mother Brain's sprite, palette, or glass artwork.</returns>
    /// <remarks>Stock provenance, hash, and compiled visual equality are checked even with overrides. Override JSON must cover every compiled identity/run shape with twelve-bit visual words; it needs no manifest and cannot change physical geometry.</remarks>
    /// <exception cref="InvalidDataException">Integrity, provenance, JSON version, frame coverage/identity, dimensions, or visual word bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmMotherBrainFakeDeathVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Mother Brain fake-death room",
            MotherBrainFakeDeathPlmDrawDefinitions.All,
            MotherBrainFakeDeathPlmDrawDefinitions.VisualId,
            entries => new RoomPlmMotherBrainFakeDeathVisualCatalog(
                entries.Select(entry =>
                    new RoomPlmMotherBrainFakeDeathVisualEntry(
                        entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Checks stock fake-death room assets through <see cref="Load"/> without selecting overrides, reading a cartridge, or writing files.</summary>
    /// <param name="directory">Family directory containing fake-death room JSON and its manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled visual checks fail.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
