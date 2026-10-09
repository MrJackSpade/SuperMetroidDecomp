using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable blue-door cap visual block references.</summary>
public static class RoomPlmBlueDoorVisualFiles
{
    /// <summary>Editable blue-door cap and shared clear-frame JSON filename.</summary>
    public const string VisualFileName = "blue-doors.json";

    /// <summary>Verifies every editable native blue-door draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Blue-door", BlueDoorPlmDrawDefinitions.Editable,
            BlueDoorPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable blue-door override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and blue-door JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose blue-door JSON is selected when present.</param>
    /// <returns>The immutable selected blue-door visual catalog.</returns>
    public static RoomPlmBlueDoorVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Blue-door", BlueDoorPlmDrawDefinitions.Editable,
            BlueDoorPlmDrawDefinitions.VisualId,
            entries => new RoomPlmBlueDoorVisualCatalog(entries.Select(entry =>
                new RoomPlmBlueDoorVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, _, block) => catalog.GetWord(pointer, block));

    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled blue-door visuals.</summary>
    /// <param name="directory">Stock blue-door directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
