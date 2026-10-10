namespace SuperMetroid.ResourceAudit;

/// <summary>Individually reviewed room-actor artwork contracts; unrelated PLM families remain unresolved.</summary>
internal static class PlmActorClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs", "1DA6DFC4917DCCCBD291864836978A1545C55A3D3C03A81B20D943885938BEAC");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmDownwardGateVisualCatalog", "plm-downward-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDownwardGateVisualCatalog.cs", "9453A8F5F41B1C3D2F085CFE0BFBE65B78504D8ED93F6F4567CC24918B9AC202"),
             new("csharp/src/SuperMetroid.Core/Rooms/DownwardGatePlmDrawDefinitions.cs", "F8A59E2D97D38630D9BD5B80849792A75F550C0EC0349BAD21B1D1319D158A99")]),
        new("SuperMetroid.Core.Rooms.RoomPlmElevatorPlatformVisualCatalog", "plm-elevator-platform-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmElevatorPlatformVisualCatalog.cs", "F123250ED0C39D9CB0D98F8F1E7B46AEA1BB92578B6502C4FA0C125AFDEE1F29"),
             new("csharp/src/SuperMetroid.Core/Rooms/ElevatorPlatformPlmDefinitions.cs", "7FB5464DA9454495B7D443F580C640389208C7DB00A4B08DD1011EE00CDA4F28")]),
        new("SuperMetroid.Core.Rooms.RoomPlmDraygonCannonVisualCatalog", "plm-draygon-cannon-complete-reachable-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDraygonCannonVisualCatalog.cs", "EDDCB0D8F3E7C3B482D2AAB24C59DDB907B3AA31CB9A15A9B50F2EDFD41E20AA"),
             new("csharp/src/SuperMetroid.Core/Rooms/DraygonCannonPlmDrawDefinitions.cs", "CC29CBEACACD913C60B5297BC94B89E888DACABF936A7E3286640B8B3FA744F2")]),
        new("SuperMetroid.Core.Rooms.RoomPlmChozoStatueVisualCatalog", "plm-chozo-statue-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmChozoStatueVisualCatalog.cs", "3D0DA12C195768E945D61FEDA8680CCAF1D5872B5E05F12C1AD8F93348E80A75"),
             new("csharp/src/SuperMetroid.Core/Rooms/ChozoStatuePlmDrawDefinitions.cs", "159450EC836BE4AC348EA2389EB8336342E9138706B7281ED4900526F4E3D9DD")]),
        new("SuperMetroid.Core.Rooms.RoomPlmLinkedRestoreVisualCatalog", "plm-linked-restore-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreVisualCatalog.cs", "53704B2E02260EAC4478CC9818157D192E3824133D4C0B126D3710CFA79797F2"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreDrawDefinitions.cs", "3FFE535A716DD38BD18877595BCF1A5166B2C4A583D4CDFAF78ABB87F09C99D9"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombBlockRestoreDrawDefinitions.cs", "66E24FF17CC3C6A7468B1F83594D921CFD6833DE9A47EAD666E311A7CB0751C6"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmContactCrumbleRestoreDrawDefinitions.cs", "499A4AD62CD925027726FF0F2557DC6DB044EDF29B4A6E58A825A1F8D01CA4E8")]),
    ];
}
