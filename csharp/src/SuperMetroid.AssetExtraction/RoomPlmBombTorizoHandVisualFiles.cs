using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable visual references for Bomb Torizo's hand PLM.</summary>
public static class RoomPlmBombTorizoHandVisualFiles
{
    /// <summary>Editable Bomb Torizo hand PLM visual JSON filename.</summary>
    public const string VisualFileName = "bomb-torizo-hand.json";

    /// <summary>Verifies every native Bomb Torizo hand draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Bomb Torizo hand", BombTorizoHandPlmDrawDefinitions.All,
            BombTorizoHandPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable Bomb Torizo hand override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and Bomb Torizo hand JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose Bomb Torizo hand JSON is selected when present.</param>
    /// <returns>The immutable selected Bomb Torizo hand visual catalog.</returns>
    public static RoomPlmBombTorizoHandVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Bomb Torizo hand", BombTorizoHandPlmDrawDefinitions.All,
            BombTorizoHandPlmDrawDefinitions.VisualId,
            entries => new RoomPlmBombTorizoHandVisualCatalog(entries.Select(entry =>
                new RoomPlmBombTorizoHandVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Bomb Torizo hand visuals.</summary>
    /// <param name="directory">Stock Bomb Torizo hand directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
