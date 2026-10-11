namespace SuperMetroid.ResourceAudit;

/// <summary>Individually reviewed room-actor artwork contracts; unrelated PLM families remain unresolved.</summary>
internal static class PlmActorClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs", "AD425ED952C838A1FB7836858E04FF2AD66D5E475562D1220F69A9F45B29F335");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmDownwardGateVisualCatalog", "plm-downward-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDownwardGateVisualCatalog.cs", "9453A8F5F41B1C3D2F085CFE0BFBE65B78504D8ED93F6F4567CC24918B9AC202"),
             new("csharp/src/SuperMetroid.Core/Rooms/DownwardGatePlmDrawDefinitions.cs", "FA7279DDE1646126556C46A8605E95BD1E3FAA2F1C7FCC358ADCE828B5557BC3")]),
        new("SuperMetroid.Core.Rooms.RoomPlmElevatorPlatformVisualCatalog", "plm-elevator-platform-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmElevatorPlatformVisualCatalog.cs", "F123250ED0C39D9CB0D98F8F1E7B46AEA1BB92578B6502C4FA0C125AFDEE1F29"),
             new("csharp/src/SuperMetroid.Core/Rooms/ElevatorPlatformPlmDefinitions.cs", "517D82F962829FA375652C5DAA589BF779DCF9CB27FE94ECDA4968D8E95ADC56")]),
        new("SuperMetroid.Core.Rooms.RoomPlmDraygonCannonVisualCatalog", "plm-draygon-cannon-complete-reachable-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDraygonCannonVisualCatalog.cs", "EDDCB0D8F3E7C3B482D2AAB24C59DDB907B3AA31CB9A15A9B50F2EDFD41E20AA"),
             new("csharp/src/SuperMetroid.Core/Rooms/DraygonCannonPlmDrawDefinitions.cs", "6B59C00D0BF0427F32B6A6CE6D8F5C86B37A46C7BCE30F429F6D51B83B40E8BA")]),
        new("SuperMetroid.Core.Rooms.RoomPlmChozoStatueVisualCatalog", "plm-chozo-statue-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmChozoStatueVisualCatalog.cs", "3D0DA12C195768E945D61FEDA8680CCAF1D5872B5E05F12C1AD8F93348E80A75"),
             new("csharp/src/SuperMetroid.Core/Rooms/ChozoStatuePlmDrawDefinitions.cs", "D0E56D05700605B7C73E289E8910019318F8CCAFC700DDFCFE043D0D3B31982D")]),
        new("SuperMetroid.Core.Rooms.RoomPlmLinkedRestoreVisualCatalog", "plm-linked-restore-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreVisualCatalog.cs", "53704B2E02260EAC4478CC9818157D192E3824133D4C0B126D3710CFA79797F2"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreDrawDefinitions.cs", "3FFE535A716DD38BD18877595BCF1A5166B2C4A583D4CDFAF78ABB87F09C99D9"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombBlockRestoreDrawDefinitions.cs", "6A32A53E35DA241C890129658D48FBC912857A097622191021FBD04DD50FD876"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmContactCrumbleRestoreDrawDefinitions.cs", "CF4626D67A29B3A44D1E91141B09AA98C9B11FC02D58C692F15CD725EF770831")]),
    ];
}
