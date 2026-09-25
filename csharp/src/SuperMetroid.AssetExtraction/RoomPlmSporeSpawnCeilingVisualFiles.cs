using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Spore Spawn ceiling visual blocks.</summary>
public static class RoomPlmSporeSpawnCeilingVisualFiles
{
    public const string VisualFileName = "spore-spawn-ceiling.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Spore Spawn ceiling",
            SporeSpawnCeilingPlmDrawDefinitions.All,
            SporeSpawnCeilingPlmDrawDefinitions.VisualId);

    public static RoomPlmSporeSpawnCeilingVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Spore Spawn ceiling",
            SporeSpawnCeilingPlmDrawDefinitions.All,
            SporeSpawnCeilingPlmDrawDefinitions.VisualId,
            entries => new RoomPlmSporeSpawnCeilingVisualCatalog(entries.Select(entry =>
                new RoomPlmSporeSpawnCeilingVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
