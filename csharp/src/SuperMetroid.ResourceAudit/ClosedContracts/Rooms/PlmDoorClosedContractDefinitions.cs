using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Source-reviewed flat door and single-word PLM artwork; aliases are owned by compiled definitions.</summary>
internal static class PlmDoorClosedContractDefinitions
{
    /// <summary>Reviewed draw-list shape source shared by blue, colored, grey, eye, and escape-gate contracts.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmShotBlockDrawDefinitions.cs");
    /// <summary>The shot-block draw logic and the shared PLM level words the draw shapes read.</summary>
    private static readonly ReviewedSource SharedDrawLogic = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/ShotBlockPlmDraws.cs");
    /// <summary>The level words PLM draws share.</summary>
    private static readonly ReviewedSource SharedLevelWords = new(
        "csharp/src/SuperMetroid.Core/Rooms/Plms/PlmLevelWordDefinitions.cs");

    /// <summary>Closed audits for five door families plus collectible and grapple-block single-word art.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmBlueDoorVisuals", "plm-blue-door-complete-aliased-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmBlueDoorVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/BlueDoorPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/BlueDoorCaps.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmColoredDoorVisuals", "plm-colored-door-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmColoredDoorVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/ColoredDoorPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/ColoredDoorCaps.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmGreyDoorVisuals", "plm-grey-door-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmGreyDoorVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/GreyDoorPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/GreyDoorCaps.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmEyeDoorVisuals", "plm-eye-door-complete-mirrored-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmEyeDoorVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorPlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorDraws.cs")]),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomPlmEscapeGateVisualCatalog", "plm-escape-gate-complete-draws", ["GetWord"],
            [SharedDrawShape, SharedDrawLogic, SharedLevelWords,
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/RoomPlmEscapeGateVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainEscapeGatePlmDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainEscapeGateDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/EscapeGateArtwork.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmCollectibleVisuals", "plm-collectible-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmCollectibleVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmCollectibleDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/CollectiblePlmDraws.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.InstalledRoomPlmGrappleBlockVisuals", "plm-grapple-block-complete-single-words", ["GetWord"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Plms/InstalledRoomPlmGrappleBlockVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmGrappleBlockDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/GrappleBlockPlmDraws.cs")]),
    ];
}
