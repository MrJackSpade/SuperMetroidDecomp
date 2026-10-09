namespace SuperMetroid.ResourceAudit;

/// <summary>Complete terrain-reveal, installed overlay, and permanent-item stores.</summary>
internal static class RoomRevealClosedContractDefinitions
{
    private static readonly ReviewedSource XrayCatalog = new(
        "csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs", "9582691188679D18CC56B94C6733957F3B60778D6A5CFB4A4EC5DCD4247B6EAD");
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.XrayRevealVisualCatalog", "xray-command-and-visual-share-one-identity", ["Apply"],
            [XrayCatalog,
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealTable.cs", "DAF667ADFD0D2095DC0A6123A1D5D8FF3DF467B7EA566890DAD3BD2591944892"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealDefinitions.cs", "8C9A46EDD4EFE6BCE1B5F8F735D17D3C2771A356D3AE0CEA87ABB6B76E05F85A"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealCodePointers.cs", "86EC8000705FAFF078C2A9C8264A2ECFE81172DC47C5F6DDF6FCFCD6A0A101FE"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomLevelWord.cs", "76E66EDE002E9700C0CBC0987C44DA5EC988A3318193A38B7ED389C601D78121")]),
        new("SuperMetroid.Core.Rooms.XrayOverlayVisualCatalog", "xray-complete-items-and-required-room-overlays", ["ItemMetatile", "RoomTiles"],
            [XrayCatalog, new("csharp/src/SuperMetroid.Core/Rooms/XrayOverlayRomData.cs", "07721A2212E9940BAEE87A39FB9173633668385EC5B726714C88A482860ABC90"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRoomOverlaySourceDefinitions.cs", "B270E150681D3043CD0DEDF501522C19286B4C2AD35CABED1C15C5F3EB82478E"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "93A78AC57BDA8AB714CA6D250C9A3CA7008EF1F254D2047BA4277ACB1A9AEFD6"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs", "42868C1BEC2CBEAEADDB79C45774FA21152564E70BB5FFD6F4D8A57B39DA53CB")]),
        new("SuperMetroid.Core.Rooms.RoomPlmDynamicCollectibleArtCatalog", "plm-complete-seventeen-item-uploads", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleArtCatalog.cs", "8255665E14E54C25F8CCB58EB2626032004B80B1C3EA6D849400012DD58D7D15"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleGraphicsDefinitions.cs", "C0213A2C783A110FDB9E76DF28B5D082E45393F2F3832F89CD2AA68E4D39A391"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleGraphicsDefinitions.Generated.cs", "564B749407C707DE90156B387B05E92940BEE9A39A41317C043E87F6C2E1A15E"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSystem.Collectibles.cs", "AA37C4703E54550A713A87315CBA11505195D990994057ED7B8256454A152507"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmHeaders.cs", "2DE013B89FB74240E47032E5EFBEF83F51A45705195745DB8F82C929FEA9F089")]),
    ];
}
