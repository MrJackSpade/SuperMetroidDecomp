using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable grey-door and shared clear-cap visual references.</summary>
public static class RoomPlmGreyDoorVisualFiles
{
    public const string VisualFileName = "grey-doors.json";
    public const string ManifestFileName = RoomPlmDoorCapVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorCapVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Grey-door", GreyDoorPlmDrawDefinitions.All,
            GreyDoorPlmDrawDefinitions.VisualId);

    public static RoomPlmGreyDoorVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorCapVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Grey-door", GreyDoorPlmDrawDefinitions.All,
            GreyDoorPlmDrawDefinitions.VisualId,
            entries => new RoomPlmGreyDoorVisualCatalog(entries.Select(entry =>
                new RoomPlmGreyDoorVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, block) => catalog.GetWord(pointer, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
