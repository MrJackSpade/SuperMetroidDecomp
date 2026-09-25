using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Cartridge-checked stock and strict visual-only eye-door overrides.</summary>
public static class RoomPlmEyeDoorVisualFiles
{
    public const string VisualFileName = "eye-doors.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "eye-door", EyeDoorPlmDrawDefinitions.All,
            EyeDoorPlmDrawDefinitions.VisualId);

    public static RoomPlmEyeDoorVisualCatalog Load(string stockDirectory,
        string? overrideDirectory = null) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "eye-door", EyeDoorPlmDrawDefinitions.All,
            EyeDoorPlmDrawDefinitions.VisualId,
            entries => new RoomPlmEyeDoorVisualCatalog(entries.Select(entry =>
                new RoomPlmEyeDoorVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, block) => catalog.GetWord(pointer, block));

    public static void ValidateStock(string stockDirectory) => Load(stockDirectory);
}
