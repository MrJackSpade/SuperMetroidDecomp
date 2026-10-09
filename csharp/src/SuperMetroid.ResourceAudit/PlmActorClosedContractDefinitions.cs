namespace SuperMetroid.ResourceAudit;

/// <summary>Individually reviewed room-actor artwork contracts; unrelated PLM families remain unresolved.</summary>
internal static class PlmActorClosedContractDefinitions
{
    /// <summary>Fingerprint for the shared source shape used by reviewed PLM actor draw contracts.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomPlmShotBlockDrawDefinitions.cs",
        "EE5DD1AAFD6BCCA4627D2D11582BCB88782327B9B764DAA7081B5DAD3822D132");

    /// <summary>Reviewed fingerprints that bound the selected room-actor PLM artwork providers.</summary>
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
             new("csharp/src/SuperMetroid.Core/Rooms/DraygonCannonPlmDrawDefinitions.cs", "264135A6B37B64FA0B5A007F9B10246FE69E7513003A5312B78CA611785A13BF")]),
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
