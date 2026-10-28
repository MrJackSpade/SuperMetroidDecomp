using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Source-reviewed flat door and single-word PLM artwork; aliases are owned by compiled definitions.</summary>
internal static class PlmDoorClosedContractDefinitions
{
    /// <summary>Reviewed draw-list shape source shared by blue, colored, grey, eye, and escape-gate contracts.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmShotBlockDrawDefinitions.cs");

    /// <summary>Closed audits for five door families plus collectible and grapple-block single-word art.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmBlueDoorVisualCatalog", "plm-blue-door-complete-aliased-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmBlueDoorVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/BlueDoorPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/BlueDoorCaps.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmColoredDoorVisualCatalog", "plm-colored-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmColoredDoorVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/ColoredDoorPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/ColoredDoorCaps.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmGreyDoorVisualCatalog", "plm-grey-door-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmGreyDoorVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/GreyDoorPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/GreyDoorCaps.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmEyeDoorVisualCatalog", "plm-eye-door-complete-mirrored-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmEyeDoorVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorDraws.cs")]),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmEscapeGateVisualCatalog", "plm-escape-gate-complete-draws", ["GetWord"],
            [SharedDrawShape,
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/RoomPlmEscapeGateVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainEscapeGatePlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainEscapeGateDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/EscapeGateArtwork.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmCollectibleVisualCatalog", "plm-collectible-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmCollectibleVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmCollectibleDrawDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmGrappleBlockVisualCatalog", "plm-grapple-block-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmGrappleBlockVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmGrappleBlockDrawDefinitions.cs")]),
    ];
}
