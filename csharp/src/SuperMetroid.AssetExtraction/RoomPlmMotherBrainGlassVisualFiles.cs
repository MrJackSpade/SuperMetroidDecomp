using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Cartridge-checked stock and strict visual-only glass overrides.</summary>
public static class RoomPlmMotherBrainGlassVisualFiles
{
    public const string VisualFileName = "mother-brain-glass.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Mother Brain glass", MotherBrainGlassPlmDrawDefinitions.All,
            MotherBrainGlassPlmDrawDefinitions.VisualId);

    public static RoomPlmMotherBrainGlassVisualCatalog Load(string stockDirectory,
        string? overrideDirectory = null) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Mother Brain glass", MotherBrainGlassPlmDrawDefinitions.All,
            MotherBrainGlassPlmDrawDefinitions.VisualId,
            entries => new RoomPlmMotherBrainGlassVisualCatalog(entries.Select(entry =>
                new RoomPlmMotherBrainGlassVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string stockDirectory) => Load(stockDirectory);
}
