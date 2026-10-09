using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Kraid ceiling and spike visual blocks.</summary>
public static class RoomPlmKraidVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for Kraid's ten ceiling and spike layouts, including the fifteen- and twenty-two-block clears.</summary>
    public const string VisualFileName = "kraid-room.json";

    /// <summary>Checks Kraid's bank-$84 draw runs, physical words, and offsets against compiled definitions, then exports presentation-only block/flip words.</summary>
    /// <param name="bus">Cartridge source for the reachable ceiling/spike draws at $84:9367–93EE, excluding the unused $938B record.</param>
    /// <param name="directory">Family asset directory, created if absent; receives new <see cref="VisualFileName"/> and <c>manifest.json</c> files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash; a later stock load requires the supported cartridge hash.</param>
    /// <remarks>No existing output is overwritten; separate JSON/manifest writes can leave the first file behind if the second fails.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">A native draw differs from its compiled shape, words, or continuation offset.</exception>
    /// <exception cref="IOException">An output already exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Kraid room",
            KraidRoomPlmDrawDefinitions.All,
            KraidRoomPlmDrawDefinitions.VisualId);

    /// <summary>Checks the installed stock Kraid layouts and selects a complete optional artwork replacement without changing collision or crumble timing.</summary>
    /// <param name="stockDirectory">Kraid family directory containing stock JSON and its version-one provenance/hash manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; missing override JSON or null selects validated stock.</param>
    /// <returns>A ROM-independent catalog of the ten compiled Kraid layouts; the separately installed elevatube appearance is not changed.</returns>
    /// <remarks>Stock is always checked against supported-cartridge provenance, file hash, and compiled visual words. Overrides replace the whole family and require every identity and original cell count; they need no manifest.</remarks>
    /// <exception cref="InvalidDataException">Integrity, provenance, document version, identities, coverage, shapes, or visual-only words are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmKraidVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Kraid room",
            KraidRoomPlmDrawDefinitions.All,
            KraidRoomPlmDrawDefinitions.VisualId,
            entries => new RoomPlmKraidVisualCatalog(entries.Select(entry =>
                new RoomPlmKraidVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Validates the stock Kraid installation through <see cref="Load"/> with no override, cartridge reads, or file writes.</summary>
    /// <param name="directory">Family directory containing the Kraid visual JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
