using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Speed Booster bomb-reveal visual block.</summary>
public static class RoomPlmSpeedBoosterVisualFiles
{
    /// <summary>Editable Speed Booster bomb-reveal visual JSON filename.</summary>
    public const string VisualFileName = "speed-booster.json";

    /// <summary>Verifies the native Speed Booster bomb-reveal draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparison.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Speed Booster", SpeedBoosterBlockPlmDrawDefinitions.All,
            SpeedBoosterBlockPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable Speed Booster override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and Speed Booster JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose Speed Booster JSON is selected when present.</param>
    /// <returns>The immutable selected Speed Booster visual catalog.</returns>
    public static RoomPlmSpeedBoosterVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Speed Booster", SpeedBoosterBlockPlmDrawDefinitions.All,
            SpeedBoosterBlockPlmDrawDefinitions.VisualId,
            entries => new RoomPlmSpeedBoosterVisualCatalog(entries.Select(entry =>
                new RoomPlmSpeedBoosterVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with the compiled Speed Booster visual.</summary>
    /// <param name="directory">Stock Speed Booster directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
