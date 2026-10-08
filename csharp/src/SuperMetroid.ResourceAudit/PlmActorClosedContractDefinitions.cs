namespace SuperMetroid.ResourceAudit;

/// <summary>Individually reviewed room-actor artwork contracts; unrelated PLM families remain unresolved.</summary>
internal static class PlmActorClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs",
        "B0121BABA88C41ABFFC02877AF97FC02C30C2F06AA4301A19B2BE00F992D835F");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmDownwardGateVisualCatalog", "plm-downward-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDownwardGateVisualCatalog.cs", "BDF32F8FC3CC81182615F089F00FAC676CEF8223B7E2D44724DD64DE2957D1F8"),
             new("csharp/src/SuperMetroid.Core/Rooms/DownwardGatePlmDrawDefinitions.cs", "A852FEC373F0016DAB0B5E0179B00611964EF7B11FF3D4171AF69864E7EC4D1A")]),
        new("SuperMetroid.Core.Rooms.RoomPlmElevatorPlatformVisualCatalog", "plm-elevator-platform-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmElevatorPlatformVisualCatalog.cs", "3744F41323CFF2906F853B9960C2A666ED5444F11528C01E97E101EEFEE06F45"),
             new("csharp/src/SuperMetroid.Core/Rooms/ElevatorPlatformPlmDefinitions.cs", "E3046A36AB0BB92585F0DD6A6215F822C4A2131ACA0F8367F9B9F84C598DB72C")]),
        new("SuperMetroid.Core.Rooms.RoomPlmDraygonCannonVisualCatalog", "plm-draygon-cannon-complete-reachable-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDraygonCannonVisualCatalog.cs", "E5BA3C70F70F9C9DE480D8728E9183313BD736602B160E63BB39115F9CEFAC60"),
             new("csharp/src/SuperMetroid.Core/Rooms/DraygonCannonPlmDrawDefinitions.cs", "96375C516827333B0B8149DA410E51D316AD3BDB085D2D6DEE64A39DEA896CFE")]),
        new("SuperMetroid.Core.Rooms.RoomPlmChozoStatueVisualCatalog", "plm-chozo-statue-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmChozoStatueVisualCatalog.cs", "ABCC452F2D16C5B9304749AF1295D9805ABAA7C71C5303D39B0ECCBA3CF80168"),
             new("csharp/src/SuperMetroid.Core/Rooms/ChozoStatuePlmDrawDefinitions.cs", "8472742279E40A1E081F658CAC49A43BAF56640EB7B3E72FA1CC862818D1D8F2")]),
        new("SuperMetroid.Core.Rooms.RoomPlmLinkedRestoreVisualCatalog", "plm-linked-restore-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreVisualCatalog.cs", "6AC19D57C02A814ECC3258F79130BF4FA3A8FEAC7145B4B9A05212EBF0B55EE1"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreDrawDefinitions.cs", "BC8E003FB292F08AC1284CFB148E11FD42BA1955718F38F1190538B86DEA9C49"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombBlockRestoreDrawDefinitions.cs", "5E67014D16D23CC2F4C0FE44B5D2B68901616D1B3312BD3740ACB7638F358EE5"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmContactCrumbleRestoreDrawDefinitions.cs", "D234274DA9CE062CD154951E4926955BBCB5C32086A5C5541E5EEF5459838127")]),
    ];
}
