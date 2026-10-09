using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Botwoon wall-clear visual blocks.</summary>
public static class RoomPlmBotwoonWallVisualFiles
{
    /// <summary>Editable Botwoon wall-clear visual JSON filename.</summary>
    public const string VisualFileName = "botwoon-wall.json";

    /// <summary>Verifies every native Botwoon wall draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Botwoon wall",
            BotwoonWallPlmDrawDefinitions.All,
            BotwoonWallPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable Botwoon wall override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and Botwoon wall JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose Botwoon wall JSON is selected when present.</param>
    /// <returns>The immutable selected Botwoon wall visual catalog.</returns>
    public static RoomPlmBotwoonWallVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Botwoon wall",
            BotwoonWallPlmDrawDefinitions.All,
            BotwoonWallPlmDrawDefinitions.VisualId,
            entries => new RoomPlmBotwoonWallVisualCatalog(entries.Select(entry =>
                new RoomPlmBotwoonWallVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Botwoon wall visuals.</summary>
    /// <param name="directory">Stock Botwoon wall directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
