namespace SuperMetroid.ResourceAudit;

/// <summary>Complete terrain-reveal, installed overlay, and permanent-item stores.</summary>
internal static class RoomRevealClosedContractDefinitions
{
    /// <summary>Stores xray catalog.</summary>
    private static readonly ReviewedSource XrayCatalog = new(
        "csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs");
    /// <summary>Stores all.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.XrayRevealVisualCatalog", "xray-command-and-visual-share-one-identity", ["Apply"],
            [XrayCatalog,
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealTable.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealCodePointers.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomLevelWord.cs")]),
        new("SuperMetroid.Core.Rooms.XrayOverlayVisualCatalog", "xray-complete-items-and-required-room-overlays", ["ItemMetatile", "RoomTiles"],
            [XrayCatalog, new("csharp/src/SuperMetroid.Core/Rooms/XrayOverlayRomData.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRoomOverlaySourceDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs")]),
        new("SuperMetroid.Core.Rooms.RoomPlmDynamicCollectibleArtCatalog", "plm-complete-seventeen-item-uploads", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleArtCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleGraphicsDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleGraphicsDefinitions.Generated.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/CollectiblePlms.cs"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmHeaders.cs")]),
    ];
}
