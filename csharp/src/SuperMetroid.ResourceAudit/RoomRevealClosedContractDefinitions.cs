namespace SuperMetroid.ResourceAudit;

/// <summary>Complete terrain-reveal, installed overlay, and permanent-item stores.</summary>
internal static class RoomRevealClosedContractDefinitions
{
    private static readonly ReviewedSource XrayCatalog = new(
        "csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs", "59C7D91414102EB118B6A61ABACAFFEEFA63DA999468F4A3272942B04770F987");
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.XrayRevealVisualCatalog", "xray-command-and-visual-share-one-identity", ["Apply"],
            [XrayCatalog,
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealTable.cs", "4D686FABDD811AAA87C100570BBC53A5E944892B08C0A89CA62619A1F9514C93"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealDefinitions.cs", "784CF048E40C02B8C3D679E679381BAE870709AE71CFBEBCFC9EA73990094520"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealCodePointers.cs", "DBB91BA49C7DEC14C71DFA9FE9CA740EEA4ADAA73D6FCB2A101164E7515A07B7"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomLevelWord.cs", "76E66EDE002E9700C0CBC0987C44DA5EC988A3318193A38B7ED389C601D78121")]),
        new("SuperMetroid.Core.Rooms.XrayOverlayVisualCatalog", "xray-complete-items-and-required-room-overlays", ["ItemMetatile", "RoomTiles"],
            [XrayCatalog, new("csharp/src/SuperMetroid.Core/Rooms/XrayOverlayRomData.cs", "07721A2212E9940BAEE87A39FB9173633668385EC5B726714C88A482860ABC90"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRoomOverlaySourceDefinitions.cs", "B270E150681D3043CD0DEDF501522C19286B4C2AD35CABED1C15C5F3EB82478E"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "A81815A52EF266CE17FB4EE569D9E07B1A0F67D47F0F914591E5D5FD0E92B159"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs", "8A23BCAE4F3DC70AAF97817F3E7BD5A12C89B320477BAEB99CFDFCB4827E1378")]),
        new("SuperMetroid.Core.Rooms.RoomPlmDynamicCollectibleArtCatalog", "plm-complete-seventeen-item-uploads", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleArtCatalog.cs", "8255665E14E54C25F8CCB58EB2626032004B80B1C3EA6D849400012DD58D7D15"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleGraphicsDefinitions.cs", "0A6F6D9BDAC5955463E3192F1B24733819F5231C16E565414BEF44A9E022DCBA"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleGraphicsDefinitions.Generated.cs", "564B749407C707DE90156B387B05E92940BEE9A39A41317C043E87F6C2E1A15E"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSystem.Collectibles.cs", "AA37C4703E54550A713A87315CBA11505195D990994057ED7B8256454A152507"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmHeaders.cs", "2DE013B89FB74240E47032E5EFBEF83F51A45705195745DB8F82C929FEA9F089")]),
    ];
}
