using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable visual references for Chozo statue terrain PLMs.</summary>
public static class RoomPlmChozoStatueVisualFiles
{
    public const string VisualFileName = "chozo-statues.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Chozo statue", ChozoStatuePlmDrawDefinitions.All,
            ChozoStatuePlmDrawDefinitions.VisualId);

    public static RoomPlmChozoStatueVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Chozo statue", ChozoStatuePlmDrawDefinitions.All,
            ChozoStatuePlmDrawDefinitions.VisualId,
            entries => new RoomPlmChozoStatueVisualCatalog(entries.Select(entry =>
                new RoomPlmChozoStatueVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
