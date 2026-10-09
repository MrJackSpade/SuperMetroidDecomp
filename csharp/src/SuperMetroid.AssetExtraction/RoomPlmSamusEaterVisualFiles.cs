using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts and loads the eight replaceable Samus Eater block frames.</summary>
public static class RoomPlmSamusEaterVisualFiles
{
    /// <summary>Editable Samus Eater plant visual JSON filename.</summary>
    public const string VisualFileName = "samus-eater.json";

    /// <summary>Verifies every native Samus Eater plant draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Samus Eater plant",
            SamusEaterPlmDrawDefinitions.All,
            SamusEaterPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable Samus Eater override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and Samus Eater JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose Samus Eater JSON is selected when present.</param>
    /// <returns>The immutable selected Samus Eater visual catalog.</returns>
    public static RoomPlmSamusEaterVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Samus Eater plant",
            SamusEaterPlmDrawDefinitions.All,
            SamusEaterPlmDrawDefinitions.VisualId,
            entries => new RoomPlmSamusEaterVisualCatalog(entries.Select(entry =>
                new RoomPlmSamusEaterVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Samus Eater visuals.</summary>
    /// <param name="directory">Stock Samus Eater directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
