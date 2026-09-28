using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts and loads the eight replaceable Samus Eater block frames.</summary>
public static class RoomPlmSamusEaterVisualFiles
{
    public const string VisualFileName = "samus-eater.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Samus Eater plant",
            SamusEaterPlmDrawDefinitions.All,
            SamusEaterPlmDrawDefinitions.VisualId);

    public static RoomPlmSamusEaterVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Samus Eater plant",
            SamusEaterPlmDrawDefinitions.All,
            SamusEaterPlmDrawDefinitions.VisualId,
            entries => new RoomPlmSamusEaterVisualCatalog(entries.Select(entry =>
                new RoomPlmSamusEaterVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
