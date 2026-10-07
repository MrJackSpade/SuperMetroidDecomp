using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using System.Text;

internal static partial class AssetTools
{
    /// <summary>One-time code generator for typed FX setup data from the pinned retail ROM.</summary>
    private static void GenerateRoomFxRecordDefinitions()
    {
        if (File.Exists(RoomFxRecordCatalogSource.GeneratedPath))
            throw new IOException($"Refusing to overwrite existing compiled definitions: {RoomFxRecordCatalogSource.GeneratedPath}");
        var bus = LoadRepositoryRom();
        SortedDictionary<ushort, RoomFxRecordDefinition> records = RoomFxRecordCatalogSource.CaptureRetailRoomFxRecords(bus);
        File.WriteAllText(RoomFxRecordCatalogSource.GeneratedPath, RoomFxRecordCatalogSource.RenderRoomFxDefinitions(records.Values));
        Console.WriteLine($"Generated {records.Count} typed room-FX records at {RoomFxRecordCatalogSource.GeneratedPath}.");
    }
}
