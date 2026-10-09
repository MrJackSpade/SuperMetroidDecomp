using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable visual references for Chozo statue terrain PLMs.</summary>
public static class RoomPlmChozoStatueVisualFiles
{
    /// <summary>JSON filename for the cleared Chozo hand and Wrecked Ship slope-access appearances.</summary>
    public const string VisualFileName = "chozo-statues.json";

    /// <summary>Verifies the three native Chozo layouts and creates flattened visual-word JSON plus its provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of full level words, run counts, and signed offsets.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled Chozo definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Chozo statue", ChozoStatuePlmDrawDefinitions.All,
            ChozoStatuePlmDrawDefinitions.VisualId);

    /// <summary>Validates stock Chozo art against its manifest and compiled layouts, then selects a complete optional visual-only override without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the Chozo JSON and its provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory whose Chozo JSON replaces stock when present; an absent file retains stock.</param>
    /// <returns>The selected hand and slope-access catalog, with native run geometry and collision unchanged.</returns>
    /// <exception cref="InvalidDataException">Stock provenance, digest, format, or compiled equality fails, or selected entries violate layout coverage or visual-word rules.</exception>
    public static RoomPlmChozoStatueVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Chozo statue", ChozoStatuePlmDrawDefinitions.All,
            ChozoStatuePlmDrawDefinitions.VisualId,
            entries => new RoomPlmChozoStatueVisualCatalog(entries.Select(entry =>
                new RoomPlmChozoStatueVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Checks stock Chozo provenance, digest, schema, complete layout coverage, and equality with compiled visual words; ignores overrides.</summary>
    /// <param name="directory">Installed stock Chozo directory.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
