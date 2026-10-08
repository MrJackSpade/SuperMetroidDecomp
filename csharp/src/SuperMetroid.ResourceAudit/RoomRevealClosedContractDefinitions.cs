namespace SuperMetroid.ResourceAudit;

/// <summary>Complete terrain-reveal, installed overlay, and permanent-item stores.</summary>
internal static class RoomRevealClosedContractDefinitions
{
    private static readonly ReviewedSource XrayCatalog = new(
        "csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs", "8C746D49DC6230D7CFF3394418CE0082EB96A4F7612FC84A82C3024462866F38");
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.XrayRevealVisualCatalog", "xray-command-and-visual-share-one-identity", ["Apply"],
            [XrayCatalog,
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealTable.cs", "BA1BF2A7B4071AE4425FC53944922252C6EC9CFBDC44AC5F2F31D26956CCD4A9"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealDefinitions.cs", "2A71F1DA3CF4A49A5C355BD92B3A92A116E9CB088AEB700C3D7DFF037FF4AC29"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealCodePointers.cs", "87502E951FBA43CD3FBD17816FF3AB2CEDDB23943B1E9EC92CDB56B25CC23DB9"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomLevelWord.cs", "C60A0D88B80F72A103567E9311D776592A330E4567D0934EF8D525B8BDA9CBD1")]),
        new("SuperMetroid.Core.Rooms.XrayOverlayVisualCatalog", "xray-complete-items-and-required-room-overlays", ["ItemMetatile", "RoomTiles"],
            [XrayCatalog, new("csharp/src/SuperMetroid.Core/Rooms/XrayOverlayRomData.cs", "AA2E43C1A69E6A3E643B779D30026FC182F83B07E000BA7253EC0342E3A1D0DD"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRoomOverlaySourceDefinitions.cs", "98E13B5B5BDBB4B9045DE9738C26A9B20C6015B7FB6DCF2A187DEBF195257F30"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "62A0435AE18B4C24973F36620FC79500DE554A06F73077794BDD66F3D98DFB53"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs", "456E784B457DC4B8361279DDA9069B6B039AA160EF819498EF60DF7F410A1D4B")]),
        new("SuperMetroid.Core.Rooms.RoomPlmDynamicCollectibleArtCatalog", "plm-complete-seventeen-item-uploads", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleArtCatalog.cs", "8A70DAE86ABF723E875335A3F443323D152A593BC66B523E59DA551B10D18E3A"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleGraphicsDefinitions.cs", "AACC9D5CC27B974EDD9CC76C0ACB6DECCB4F2DDBBC20B08A7E740827D7D650FF"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleGraphicsDefinitions.Generated.cs", "DF3A7FC53C7DDBB57A8CA16FD3BE6E6EE91F057A86A2C4F458D3F036D45AC782"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSystem.Collectibles.cs", "5BD9B6917BB21DB972D7A56A6FB8DF4CB5C9F462F1D567EDA99EA4431D369971"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmHeaders.cs", "0D9279D0FBF9FF72981B929A9466B45B121BD0A2F418014740D9DF66D9E8E973")]),
    ];
}
