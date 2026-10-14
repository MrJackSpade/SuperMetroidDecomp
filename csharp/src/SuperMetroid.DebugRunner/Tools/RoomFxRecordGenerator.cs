using SuperMetroid.Core.Fx;
namespace SuperMetroid.DebugRunner.Tools;
using SuperMetroid.TestSupport.CatalogInventories;

/// <summary>Captures retail room-FX records into a compiled source file only when no generated file exists.</summary>
internal static class RoomFxRecordGenerator
{
    /// <summary>One-time code generator for typed FX setup data from the pinned retail ROM.</summary>
    internal static void GenerateRoomFxRecordDefinitions()
    {
        if (File.Exists(RoomFxRecordCatalogSource.GeneratedPath))
            throw new IOException($"Refusing to overwrite existing compiled definitions: {RoomFxRecordCatalogSource.GeneratedPath}");
        var bus = RepositoryRom.Load();
        SortedDictionary<ushort, RoomFxRecordDefinition> records = RoomFxRecordCatalogSource.CaptureRetailRoomFxRecords(bus);
        File.WriteAllText(RoomFxRecordCatalogSource.GeneratedPath, RoomFxRecordCatalogSource.RenderRoomFxDefinitions(records.Values));
        Console.WriteLine($"Generated {records.Count} typed room-FX records at {RoomFxRecordCatalogSource.GeneratedPath}.");
    }
}
