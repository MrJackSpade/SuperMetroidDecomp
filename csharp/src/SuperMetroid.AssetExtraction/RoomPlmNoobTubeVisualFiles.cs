using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Cartridge-checked stock and strict visual-only n00b-tube overrides.</summary>
public static class RoomPlmNoobTubeVisualFiles
{
    public const string VisualFileName = "noob-tube.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "n00b-tube", NoobTubePlmDrawDefinitions.All,
            NoobTubePlmDrawDefinitions.VisualId);

    public static RoomPlmNoobTubeVisualCatalog Load(string stockDirectory,
        string? overrideDirectory = null) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "n00b-tube", NoobTubePlmDrawDefinitions.All,
            NoobTubePlmDrawDefinitions.VisualId,
            entries => new RoomPlmNoobTubeVisualCatalog(entries.Select(entry =>
                new RoomPlmNoobTubeVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string stockDirectory) => Load(stockDirectory);
}
