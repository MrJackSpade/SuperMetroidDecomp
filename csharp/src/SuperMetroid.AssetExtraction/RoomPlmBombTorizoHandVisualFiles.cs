using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Installed, replaceable visual references for Bomb Torizo's hand PLM.</summary>
public static class RoomPlmBombTorizoHandVisualFiles
{
    public const string VisualFileName = "bomb-torizo-hand.json";
    public const string ManifestFileName = RoomPlmDoorVisualFileCodec.ManifestFileName;

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Bomb Torizo hand", BombTorizoHandPlmDrawDefinitions.All,
            BombTorizoHandPlmDrawDefinitions.VisualId);

    public static RoomPlmBombTorizoHandVisualCatalog Load(
        string stockDirectory, string? overrideDirectory) =>
        RoomPlmDoorVisualFileCodec.Load(stockDirectory, overrideDirectory,
            VisualFileName, "Bomb Torizo hand", BombTorizoHandPlmDrawDefinitions.All,
            BombTorizoHandPlmDrawDefinitions.VisualId,
            entries => new RoomPlmBombTorizoHandVisualCatalog(entries.Select(entry =>
                new RoomPlmBombTorizoHandVisualEntry(entry.Id, entry.Blocks))),
            (catalog, pointer, run, block) => catalog.GetWord(pointer, run, block));

    public static void ValidateStock(string directory) => _ = Load(directory, null);
}
