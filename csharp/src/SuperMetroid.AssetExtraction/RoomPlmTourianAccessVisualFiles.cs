using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Tourian entrance access-floor visuals.</summary>
public static class RoomPlmTourianAccessVisualFiles
{
    /// <summary>Editable Tourian entrance access-floor visual JSON filename.</summary>
    public const string VisualFileName = "tourian-access.json";

    /// <summary>Verifies every native Tourian access-floor draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Tourian access", TourianAccessPlmDrawDefinitions.All,
            TourianAccessPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable Tourian access override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and Tourian access JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose Tourian access JSON is selected when present.</param>
    /// <returns>The immutable selected Tourian access-floor visual catalog.</returns>
    public static RoomPlmTourianAccessVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Tourian access", TourianAccessPlmDrawDefinitions.All,
            TourianAccessPlmDrawDefinitions.VisualId,
            entries => new RoomPlmTourianAccessVisualCatalog(entries.Select(entry =>
                new RoomPlmTourianAccessVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled Tourian access visuals.</summary>
    /// <param name="directory">Stock Tourian access directory; overrides are ignored.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
