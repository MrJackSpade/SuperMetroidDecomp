using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Mother Brain fake-death room tile art.</summary>
public static class RoomPlmMotherBrainFakeDeathVisualFiles
{
    public const string VisualFileName = "mother-brain-fake-death.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Mother Brain fake-death room",
            MotherBrainFakeDeathPlmDrawDefinitions.All,
            MotherBrainFakeDeathPlmDrawDefinitions.VisualId);

    public static RoomPlmMotherBrainFakeDeathVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Mother Brain fake-death room",
            MotherBrainFakeDeathPlmDrawDefinitions.All,
            MotherBrainFakeDeathPlmDrawDefinitions.VisualId,
            entries => new RoomPlmMotherBrainFakeDeathVisualCatalog(
                entries.Select(entry =>
                    new RoomPlmMotherBrainFakeDeathVisualEntry(
                        entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
