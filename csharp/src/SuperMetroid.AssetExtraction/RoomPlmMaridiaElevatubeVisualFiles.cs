using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable Maridia elevatube PLM visual block.</summary>
public static class RoomPlmMaridiaElevatubeVisualFiles
{
    public const string VisualFileName = "maridia-elevatube.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Maridia elevatube",
            MaridiaElevatubePlmDefinitions.AllDraws,
            MaridiaElevatubePlmDefinitions.DrawVisualId);

    public static RoomPlmMaridiaElevatubeVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Maridia elevatube",
            MaridiaElevatubePlmDefinitions.AllDraws,
            MaridiaElevatubePlmDefinitions.DrawVisualId,
            entries => new RoomPlmMaridiaElevatubeVisualCatalog(entries.Select(entry =>
                new RoomPlmMaridiaElevatubeVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
