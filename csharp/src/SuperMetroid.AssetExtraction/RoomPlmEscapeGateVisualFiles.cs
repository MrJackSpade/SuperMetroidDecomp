using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable visual references for Mother Brain's escape gate.</summary>
public static class RoomPlmEscapeGateVisualFiles
{
    /// <summary>JSON filename for open, half-closed, and closed Mother Brain escape-gate appearances.</summary>
    public const string VisualFileName = "escape-gate.json";

    /// <summary>Verifies the three native four-block gate layouts and creates visual-only JSON plus its provenance and digest manifest.</summary>
    /// <param name="bus">Cartridge source for bank-$84 comparisons of full gate level words and four-cell vertical geometry.</param>
    /// <param name="directory">Destination directory, created if absent; the visual JSON and manifest must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source identity recorded in the manifest; subsequent loading requires the supported cartridge identity.</param>
    /// <exception cref="InvalidDataException">Native draw contents or geometry differ from the compiled escape-gate definitions.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Escape-gate", MotherBrainEscapeGatePlmDrawDefinitions.All,
            MotherBrainEscapeGatePlmDrawDefinitions.VisualId);

    /// <summary>Validates stock escape-gate art against its manifest and compiled frames, then selects a complete optional visual-only override without cartridge access.</summary>
    /// <param name="stockDirectory">Stock directory containing the escape-gate JSON and its provenance/digest manifest.</param>
    /// <param name="overrideDirectory">Optional directory whose escape-gate JSON replaces stock when present; an absent file retains stock.</param>
    /// <returns>The selected three-frame gate art; even visually open cells retain their compiled solid collision.</returns>
    /// <exception cref="InvalidDataException">Stock provenance, digest, format, or compiled equality fails, or selected entries violate frame coverage or visual-word rules.</exception>
    public static RoomPlmEscapeGateVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Escape-gate", MotherBrainEscapeGatePlmDrawDefinitions.All,
            MotherBrainEscapeGatePlmDrawDefinitions.VisualId,
            entries => new RoomPlmEscapeGateVisualCatalog(entries.Select(entry =>
                new RoomPlmEscapeGateVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, _, block) => catalog.GetWord(pointer, block));

    /// <summary>Checks stock escape-gate provenance, digest, schema, three-frame coverage, and equality with compiled visual words; ignores overrides.</summary>
    /// <param name="directory">Installed stock escape-gate directory.</param>
    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
