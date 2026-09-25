using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable visual references for reachable Draygon cannons.</summary>
public static class RoomPlmDraygonCannonVisualFiles
{
    public const string VisualFileName = "draygon-cannons.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Draygon cannon", DraygonCannonPlmDrawDefinitions.All,
            DraygonCannonPlmDrawDefinitions.VisualId);

    public static RoomPlmDraygonCannonVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Draygon cannon", DraygonCannonPlmDrawDefinitions.All,
            DraygonCannonPlmDrawDefinitions.VisualId,
            entries => new RoomPlmDraygonCannonVisualCatalog(entries.Select(entry =>
                new RoomPlmDraygonCannonVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
