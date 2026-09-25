using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable bomb and contact-crumble restore visuals.</summary>
public static class RoomPlmLinkedRestoreVisualFiles
{
    public const string VisualFileName = "linked-restores.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Linked restore", RoomPlmLinkedRestoreDrawDefinitions.All,
            RoomPlmLinkedRestoreDrawDefinitions.VisualId);

    public static RoomPlmLinkedRestoreVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Linked restore", RoomPlmLinkedRestoreDrawDefinitions.All,
            RoomPlmLinkedRestoreDrawDefinitions.VisualId,
            entries => new RoomPlmLinkedRestoreVisualCatalog(entries.Select(entry =>
                new RoomPlmLinkedRestoreVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
