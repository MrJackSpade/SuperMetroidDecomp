namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed complete bank-$84 artwork domains, separate from collision words and instruction timing.</summary>
internal static class PlmClosedContractDefinitions
{
    /// <summary>Stores shared draw shape.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs");

    /// <summary>Stores all.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmShotBlockVisualCatalog", "plm-shot-block-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockVisualCatalog.cs")]),
        new("SuperMetroid.Core.Rooms.RoomPlmStationVisualCatalog", "plm-station-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmStationDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.RoomPlmBombTorizoHandVisualCatalog", "plm-torizo-hand-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombTorizoHandVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/BombTorizoHandPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.RoomPlmMotherBrainGlassVisualCatalog", "plm-mother-brain-glass-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmMotherBrainGlassVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/MotherBrainGlassPlmDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.RoomPlmNoobTubeVisualCatalog", "plm-noob-tube-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmNoobTubeVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/NoobTubePlmDrawDefinitions.cs")]),
    ];
}
