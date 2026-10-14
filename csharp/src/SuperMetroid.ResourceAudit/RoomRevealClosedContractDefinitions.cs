namespace SuperMetroid.ResourceAudit;

/// <summary>Complete terrain-reveal, installed overlay, and permanent-item stores.</summary>
internal static class RoomRevealClosedContractDefinitions
{
    /// <summary>Reviewed source for X-ray visual ownership shared by reveal and overlay contracts.</summary>
    private static readonly ReviewedSource XrayCatalog = new(
        "csharp/src/SuperMetroid.Core/Rooms/Xray/XrayRevealVisualCatalog.cs");
    /// <summary>Names the three closed audits for reveal commands, room overlays, and collectible uploads.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.Xray.XrayRevealVisualCatalog", "xray-command-and-visual-share-one-identity", ["Apply"],
            [XrayCatalog,
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayRevealTable.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayRevealDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayRevealCodePointers.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/LevelData/RoomLevelWord.cs")]),
        new("SuperMetroid.Core.Rooms.Xray.XrayOverlayVisualCatalog", "xray-complete-items-and-required-room-overlays", ["ItemMetatile", "RoomTiles"],
            [XrayCatalog, new("csharp/src/SuperMetroid.Core/Rooms/Xray/XrayOverlayVisualCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayRoomOverlayVisual.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayOverlayRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Abilities/XrayRoomOverlaySourceDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs")]),
        new("SuperMetroid.Core.Rooms.Plms.RoomPlmDynamicCollectibleArtCatalog", "plm-complete-seventeen-item-uploads", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmDynamicCollectibleArtCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmDynamicCollectibleGraphicsDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmDynamicCollectibleGraphicsDefinitions.Generated.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/CollectiblePlms.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/Plms/RoomPlmHeaders.cs")]),
    ];
}
