using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Maridia elevatube PLM visual block.</summary>
public static class RoomPlmMaridiaElevatubeVisualFiles
{
    /// <summary>Family-relative stock/override JSON filename for the Maridia elevatube's single visual block, separate from Kraid's use of the same native draw.</summary>
    public const string VisualFileName = "maridia-elevatube.json";

    /// <summary>Checks the one-block elevatube draw at $84:9367 and exports its metatile/flip appearance without changing the solid word, sixteen-update wait, or sound.</summary>
    /// <param name="bus">Cartridge source whose native run count, $8180 physical word, and terminal offset are checked against compiled definitions.</param>
    /// <param name="directory">Family directory, created if absent; receives new <see cref="VisualFileName"/> and <c>manifest.json</c> files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank importer-supplied provenance hash; not recomputed here and required to identify the supported cartridge on stock load.</param>
    /// <remarks>Existing outputs are not overwritten; the separate JSON and manifest writes are not rolled back together if a later write fails.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">The native draw differs from its compiled shape, word, or offset.</exception>
    /// <exception cref="IOException">An output already exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Maridia elevatube",
            MaridiaElevatubePlmDefinitions.AllDraws,
            MaridiaElevatubePlmDefinitions.DrawVisualId);

    /// <summary>Validates the installed elevatube stock image and selects an optional one-block visual replacement.</summary>
    /// <param name="stockDirectory">Family directory containing stock JSON and its required version-one manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or an absent override JSON selects validated stock.</param>
    /// <returns>A ROM-independent catalog for <c>elevatube-block</c>; Kraid appearance and elevatube mechanics remain separate.</returns>
    /// <remarks>Stock must match supported-cartridge provenance, its byte hash, and compiled visual bits before overrides are considered. Override JSON needs the sole known identity and one twelve-bit visual word, but no manifest.</remarks>
    /// <exception cref="InvalidDataException">Stock integrity/provenance, JSON version, identity, coverage, block count, or visual-only bits are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
    public static RoomPlmMaridiaElevatubeVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Maridia elevatube",
            MaridiaElevatubePlmDefinitions.AllDraws,
            MaridiaElevatubePlmDefinitions.DrawVisualId,
            entries => new RoomPlmMaridiaElevatubeVisualCatalog(entries.Select(entry =>
                new RoomPlmMaridiaElevatubeVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Runs the elevatube stock checks from <see cref="Load"/> with no override, cartridge reads, or writes.</summary>
    /// <param name="directory">Family directory containing elevatube stock JSON and manifest.</param>
    /// <exception cref="InvalidDataException">Stock provenance, integrity, schema, or compiled appearance checks fail.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
