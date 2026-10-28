using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Complete terrain-reveal, installed overlay, and permanent-item stores.</summary>
internal static class RoomRevealClosedContractDefinitions
{
    /// <summary>Reviewed source for X-ray visual ownership shared by reveal and overlay contracts.</summary>
    private static readonly ReviewedSource XrayCatalog = new(
        "csharp/src/SuperMetroid.Core/Samus/Abilities/Artwork/InstalledXrayReveals.cs");
    /// <summary>Names the three closed audits for reveal commands, room overlays, and collectible uploads.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Samus.Abilities.Artwork.InstalledXrayReveals", "xray-command-and-visual-share-one-identity", ["Apply"],
            [XrayCatalog,
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayRevealTable.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayRevealDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayReveals.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomLevelWord.cs")]),
        new("SuperMetroid.Core.Samus.Abilities.Artwork.InstalledXrayOverlays", "xray-complete-items-and-required-room-overlays", ["ItemMetatile", "RoomTiles"],
            [XrayCatalog, new("csharp/src/SuperMetroid.Core/Samus/Abilities/Artwork/InstalledXrayOverlays.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayRoomOverlayVisual.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Xray/XrayRoomOverlayDefinition.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Xray/XrayRoomOverlayDefinitions.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmDynamicCollectibleArtCatalog", "plm-complete-seventeen-item-uploads", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmDynamicCollectibleArtCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmDynamicCollectibleGraphicsDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmDynamicCollectibleGraphicsDefinitions.Generated.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/CollectiblePlms.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmHeaders.cs")]),
    ];
}
