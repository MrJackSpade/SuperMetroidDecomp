using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Botwoon wall-clear visual blocks.</summary>
public static class RoomPlmBotwoonWallVisualFiles
{
    public const string VisualFileName = "botwoon-wall.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Botwoon wall",
            BotwoonWallPlmDrawDefinitions.All,
            BotwoonWallPlmDrawDefinitions.VisualId);

    public static RoomPlmBotwoonWallVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Botwoon wall",
            BotwoonWallPlmDrawDefinitions.All,
            BotwoonWallPlmDrawDefinitions.VisualId,
            entries => new RoomPlmBotwoonWallVisualCatalog(entries.Select(entry =>
                new RoomPlmBotwoonWallVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
