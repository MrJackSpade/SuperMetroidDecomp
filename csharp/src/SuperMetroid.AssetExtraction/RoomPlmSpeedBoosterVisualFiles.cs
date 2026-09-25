using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Speed Booster bomb-reveal visual block.</summary>
public static class RoomPlmSpeedBoosterVisualFiles
{
    public const string VisualFileName = "speed-booster.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Speed Booster", SpeedBoosterBlockPlmDrawDefinitions.All,
            SpeedBoosterBlockPlmDrawDefinitions.VisualId);

    public static RoomPlmSpeedBoosterVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Speed Booster", SpeedBoosterBlockPlmDrawDefinitions.All,
            SpeedBoosterBlockPlmDrawDefinitions.VisualId,
            entries => new RoomPlmSpeedBoosterVisualCatalog(entries.Select(entry =>
                new RoomPlmSpeedBoosterVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
