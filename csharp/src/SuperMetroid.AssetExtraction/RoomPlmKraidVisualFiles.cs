using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Kraid ceiling and spike visual blocks.</summary>
public static class RoomPlmKraidVisualFiles
{
    public const string VisualFileName = "kraid-room.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Kraid room",
            KraidRoomPlmDrawDefinitions.All,
            KraidRoomPlmDrawDefinitions.VisualId);

    public static RoomPlmKraidVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Kraid room",
            KraidRoomPlmDrawDefinitions.All,
            KraidRoomPlmDrawDefinitions.VisualId,
            entries => new RoomPlmKraidVisualCatalog(entries.Select(entry =>
                new RoomPlmKraidVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
