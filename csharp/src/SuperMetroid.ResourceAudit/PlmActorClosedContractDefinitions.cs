namespace SuperMetroid.ResourceAudit;

/// <summary>Individually reviewed room-actor artwork contracts; unrelated PLM families remain unresolved.</summary>
internal static class PlmActorClosedContractDefinitions
{
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs", "9985A5C60C510351018F1DDBE431A8E39C599D61233AB8F678B7946BC1290782");

    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomPlmDownwardGateVisualCatalog", "plm-downward-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDownwardGateVisualCatalog.cs", "9453A8F5F41B1C3D2F085CFE0BFBE65B78504D8ED93F6F4567CC24918B9AC202"),
             new("csharp/src/SuperMetroid.Core/Rooms/DownwardGatePlmDrawDefinitions.cs", "F8A59E2D97D38630D9BD5B80849792A75F550C0EC0349BAD21B1D1319D158A99")]),
        new("SuperMetroid.Core.Rooms.RoomPlmElevatorPlatformVisualCatalog", "plm-elevator-platform-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmElevatorPlatformVisualCatalog.cs", "F123250ED0C39D9CB0D98F8F1E7B46AEA1BB92578B6502C4FA0C125AFDEE1F29"),
             new("csharp/src/SuperMetroid.Core/Rooms/ElevatorPlatformPlmDefinitions.cs", "03DD21B0C3C87866C0FA458D5308172DD5378DD1F8A1921FAE7FB91755140647")]),
        new("SuperMetroid.Core.Rooms.RoomPlmDraygonCannonVisualCatalog", "plm-draygon-cannon-complete-reachable-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDraygonCannonVisualCatalog.cs", "EDDCB0D8F3E7C3B482D2AAB24C59DDB907B3AA31CB9A15A9B50F2EDFD41E20AA"),
             new("csharp/src/SuperMetroid.Core/Rooms/DraygonCannonPlmDrawDefinitions.cs", "0CC5EEADDA3B6025DA178363FFD89429E021CC0357408B42215EA5BE2EBFD41B")]),
        new("SuperMetroid.Core.Rooms.RoomPlmChozoStatueVisualCatalog", "plm-chozo-statue-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmChozoStatueVisualCatalog.cs", "3D0DA12C195768E945D61FEDA8680CCAF1D5872B5E05F12C1AD8F93348E80A75"),
             new("csharp/src/SuperMetroid.Core/Rooms/ChozoStatuePlmDrawDefinitions.cs", "92230D5F1C2C68761C8F1FEC8AE8E1A1F27A6CA20DBC2B5B402B22A25D449F83")]),
        new("SuperMetroid.Core.Rooms.RoomPlmLinkedRestoreVisualCatalog", "plm-linked-restore-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreVisualCatalog.cs", "53704B2E02260EAC4478CC9818157D192E3824133D4C0B126D3710CFA79797F2"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmLinkedRestoreDrawDefinitions.cs", "2A4454D0DC6F20AF865216AC660EBA71AFFBA98C4894EC75BF51B8FB6F5472E2"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmBombBlockRestoreDrawDefinitions.cs", "74FA6DE163509C7B4CA45569139920BF16A65DF97492A3EF9E5E4096A2325750"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmContactCrumbleRestoreDrawDefinitions.cs", "9A18A39D2F525F720F0E25E368B7487B57675C23D2BE993C4343ED13AF8C2F87")]),
    ];
}
