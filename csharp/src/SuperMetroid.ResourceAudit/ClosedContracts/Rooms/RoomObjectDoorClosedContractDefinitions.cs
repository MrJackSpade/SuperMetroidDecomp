using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Source-reviewed flat door and single-word PLM artwork; aliases are owned by compiled definitions.</summary>
internal static class RoomObjectDoorClosedContractDefinitions
{
    /// <summary>Reviewed draw-list shape source shared by blue, colored, grey, eye, and escape-gate contracts.</summary>
    private static readonly ReviewedSource SharedDrawShape = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectShotBlockDrawDefinitions.cs");
    /// <summary>The shot-block draw logic and the shared PLM level words the draw shapes read.</summary>
    private static readonly ReviewedSource SharedDrawLogic = new(
        "csharp/src/SuperMetroid.Core/Rooms/RoomObjects/ShotBlockObjectDraws.cs");
    /// <summary>The terrain blocks PLM draws share.</summary>
    private static readonly ReviewedSource SharedTerrainBlocks = new(
        "csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomTerrainBlocks.cs");

    /// <summary>Closed audits for five door families plus collectible and grapple-block single-word art.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectBlueDoorVisuals", "plm-blue-door-complete-aliased-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectBlueDoorVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/BlueDoorObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/BlueDoorCaps.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectColoredDoorVisuals", "plm-colored-door-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectColoredDoorVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/ColoredDoorObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/ColoredDoorCaps.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectGreyDoorVisuals", "plm-grey-door-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectGreyDoorVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/GreyDoorObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/GreyDoorCaps.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectEyeDoorVisuals", "plm-eye-door-complete-mirrored-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectEyeDoorVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Doors/EyeDoorDraws.cs")]),
        new("SuperMetroid.Core.Bosses.MotherBrain.Artwork.RoomObjectEscapeGateVisualCatalog", "plm-escape-gate-complete-draws", ["GetAppearance"],
            [SharedDrawShape, SharedDrawLogic, SharedTerrainBlocks,
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/RoomObjectEscapeGateVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainEscapeGateObjectDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainEscapeGateDraws.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/EscapeGateArtwork.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectCollectibleVisuals", "plm-collectible-complete-single-words", ["GetAppearance"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectCollectibleVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectCollectibleDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/CollectibleObjectDraws.cs")]),
        new("SuperMetroid.Core.Rooms.RoomObjects.InstalledRoomObjectGrappleBlockVisuals", "plm-grapple-block-complete-single-words", ["GetAppearance"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/InstalledRoomObjectGrappleBlockVisuals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/RoomObjectGrappleBlockDrawDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomObjects/GrappleBlockObjectDraws.cs")]),
    ];
}
