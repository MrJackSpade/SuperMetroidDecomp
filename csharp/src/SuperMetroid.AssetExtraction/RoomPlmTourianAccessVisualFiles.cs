using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Tourian entrance access-floor visuals.</summary>
public static class RoomPlmTourianAccessVisualFiles
{
    public const string VisualFileName = "tourian-access.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Tourian access", TourianAccessPlmDrawDefinitions.All,
            TourianAccessPlmDrawDefinitions.VisualId);

    public static RoomPlmTourianAccessVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Tourian access", TourianAccessPlmDrawDefinitions.All,
            TourianAccessPlmDrawDefinitions.VisualId,
            entries => new RoomPlmTourianAccessVisualCatalog(entries.Select(entry =>
                new RoomPlmTourianAccessVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
