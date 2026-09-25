using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Crocomire arena bridge and wall tile art.</summary>
public static class RoomPlmCrocomireVisualFiles
{
    public const string VisualFileName = "crocomire-arena.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Crocomire arena",
            CrocomireArenaPlmDrawDefinitions.All,
            CrocomireArenaPlmDrawDefinitions.VisualId);

    public static RoomPlmCrocomireVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Crocomire arena",
            CrocomireArenaPlmDrawDefinitions.All,
            CrocomireArenaPlmDrawDefinitions.VisualId,
            entries => new RoomPlmCrocomireVisualCatalog(entries.Select(entry =>
                new RoomPlmCrocomireVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
