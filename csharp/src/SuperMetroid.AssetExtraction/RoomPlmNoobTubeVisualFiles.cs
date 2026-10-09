using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Cartridge-checked stock and strict visual-only n00b-tube overrides.</summary>
public static class RoomPlmNoobTubeVisualFiles
{
    /// <summary>Editable n00b-tube visual JSON filename.</summary>
    public const string VisualFileName = "noob-tube.json";

    /// <summary>Verifies every native n00b-tube draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "n00b-tube", NoobTubePlmDrawDefinitions.All,
            NoobTubePlmDrawDefinitions.VisualId);

    /// <summary>Validates stock provenance and compiled parity, then selects an optional editable n00b-tube override without cartridge access.</summary>
    /// <param name="stockDirectory">Installed stock directory containing the shared manifest and n00b-tube JSON.</param>
    /// <param name="overrideDirectory">Optional directory whose n00b-tube JSON is selected when present.</param>
    /// <returns>The immutable selected n00b-tube visual catalog.</returns>
    public static RoomPlmNoobTubeVisualCatalog Load(string stockDirectory,
        string? overrideDirectory = null) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "n00b-tube", NoobTubePlmDrawDefinitions.All,
            NoobTubePlmDrawDefinitions.VisualId,
            entries => new RoomPlmNoobTubeVisualCatalog(entries.Select(entry =>
                new RoomPlmNoobTubeVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    /// <summary>Validates the installed stock manifest, digest, schema, coverage, and equality with compiled n00b-tube visuals.</summary>
    /// <param name="stockDirectory">Stock n00b-tube directory; overrides are ignored.</param>
    public static void ValidateStock(string stockDirectory) => Load(stockDirectory);
}
