using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable yellow, green, and red door-cap visual references.</summary>
public static class RoomPlmColoredDoorVisualFiles
{
    public const string VisualFileName = "colored-doors.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Colored-door", ColoredDoorPlmDrawDefinitions.All,
            ColoredDoorPlmDrawDefinitions.VisualId);

    public static RoomPlmColoredDoorVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Colored-door", ColoredDoorPlmDrawDefinitions.All,
            ColoredDoorPlmDrawDefinitions.VisualId,
            entries => new RoomPlmColoredDoorVisualCatalog(entries.Select(entry =>
                new RoomPlmColoredDoorVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, block) => catalog.GetWord(pointer, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
