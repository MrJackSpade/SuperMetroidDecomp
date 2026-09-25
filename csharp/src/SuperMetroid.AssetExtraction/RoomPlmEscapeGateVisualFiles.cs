using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable visual references for Mother Brain's escape gate.</summary>
public static class RoomPlmEscapeGateVisualFiles
{
    public const string VisualFileName = "escape-gate.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Escape-gate", MotherBrainEscapeGatePlmDrawDefinitions.All,
            MotherBrainEscapeGatePlmDrawDefinitions.VisualId);

    public static RoomPlmEscapeGateVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Escape-gate", MotherBrainEscapeGatePlmDrawDefinitions.All,
            MotherBrainEscapeGatePlmDrawDefinitions.VisualId,
            entries => new RoomPlmEscapeGateVisualCatalog(entries.Select(entry =>
                new RoomPlmEscapeGateVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, _, block) => catalog.GetWord(pointer, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
