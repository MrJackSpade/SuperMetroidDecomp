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
        RoomFxRecordCatalogSource.RetailRoomFxCapture capture = RoomFxRecordCatalogSource.CaptureRetailRoomFx(bus);
        File.WriteAllText(RoomFxRecordCatalogSource.GeneratedPath, RoomFxRecordCatalogSource.RenderRoomFxDefinitions(capture));
        Console.WriteLine($"Generated {capture.Lists.Count} room FX headers with {capture.Records.Count} records at {RoomFxRecordCatalogSource.GeneratedPath}.");
    }
}
