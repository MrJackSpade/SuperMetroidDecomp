using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Crocomire arena bridge and wall tile art.</summary>
public static class RoomPlmCrocomireVisualFiles
{
    /// <summary>JSON filename for Crocomire bridge and invisible-wall visual layouts.</summary>
    public const string VisualFileName = "crocomire-arena.json";

    /// <summary>Verifies all five native Crocomire arena layouts and creates flattened visual-word JSON plus its provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of bridge and wall level words, run shapes, and origin-relative offsets.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled arena definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Crocomire arena",
            CrocomireArenaPlmDrawDefinitions.All,
            CrocomireArenaPlmDrawDefinitions.VisualId);

    /// <summary>Validates stock Crocomire art against its manifest and compiled layouts, then selects a complete optional visual-only override without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the arena JSON and its provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory whose arena JSON replaces stock when present; an absent file retains stock.</param>
    /// <returns>The selected bridge and wall art, independent of compiled solidity, crumble programs, and placement.</returns>
    /// <exception cref="InvalidDataException">Stock provenance, digest, format, or compiled equality fails, or selected entries violate layout coverage or visual-word rules.</exception>
    public static RoomPlmCrocomireVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Crocomire arena",
            CrocomireArenaPlmDrawDefinitions.All,
            CrocomireArenaPlmDrawDefinitions.VisualId,
            entries => new RoomPlmCrocomireVisualCatalog(entries.Select(entry =>
                new RoomPlmCrocomireVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Checks stock arena provenance, digest, schema, five-layout coverage, and equality with compiled visual words; ignores overrides.</summary>
    /// <param name="directory">Installed stock Crocomire arena directory.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
