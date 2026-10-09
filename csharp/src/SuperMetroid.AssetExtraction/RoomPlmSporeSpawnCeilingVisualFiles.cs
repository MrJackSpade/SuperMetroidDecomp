using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

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
            SporeSpawnCeilingPlmDrawDefinitions.All,
            SporeSpawnCeilingPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable Spore Spawn ceiling override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and Spore Spawn ceiling JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose Spore Spawn ceiling JSON is selected when present.</param>
    /// <returns>The immutable selected Spore Spawn ceiling visual catalog.</returns>
    public static RoomPlmSporeSpawnCeilingVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Spore Spawn ceiling",
            SporeSpawnCeilingPlmDrawDefinitions.All,
            SporeSpawnCeilingPlmDrawDefinitions.VisualId,
            entries => new RoomPlmSporeSpawnCeilingVisualCatalog(entries.Select(entry =>
                new RoomPlmSporeSpawnCeilingVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Spore Spawn ceiling visuals.</summary>
    /// <param name="directory">Stock Spore Spawn ceiling directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
