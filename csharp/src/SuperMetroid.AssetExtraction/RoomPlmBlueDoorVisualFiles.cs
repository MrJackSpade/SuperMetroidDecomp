using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable blue-door cap visual block references.</summary>
public static class RoomPlmBlueDoorVisualFiles
{
    public const string VisualFileName = "blue-doors.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Blue-door", BlueDoorPlmDrawDefinitions.All,
            BlueDoorPlmDrawDefinitions.VisualId);

    public static RoomPlmBlueDoorVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Blue-door", BlueDoorPlmDrawDefinitions.All,
            BlueDoorPlmDrawDefinitions.VisualId,
            entries => new RoomPlmBlueDoorVisualCatalog(entries.Select(entry =>
                new RoomPlmBlueDoorVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, _, block) => catalog.GetWord(pointer, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
