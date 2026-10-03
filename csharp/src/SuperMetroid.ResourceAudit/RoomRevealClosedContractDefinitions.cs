namespace SuperMetroid.ResourceAudit;

/// <summary>Complete terrain-reveal, installed overlay, and permanent-item stores.</summary>
internal static class RoomRevealClosedContractDefinitions
{
    private static readonly ReviewedSource XrayCatalog = new(
        "csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs", "3134D5D237A23A4F9BE14067D3A8A0F61D604C2858A930F76A116BD158510BE9");
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Rooms.XrayRevealVisualCatalog", "xray-command-and-visual-share-one-identity", ["Apply"],
            [XrayCatalog,
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealTable.cs", "BA1BF2A7B4071AE4425FC53944922252C6EC9CFBDC44AC5F2F31D26956CCD4A9"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealDefinitions.cs", "2A71F1DA3CF4A49A5C355BD92B3A92A116E9CB088AEB700C3D7DFF037FF4AC29"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRevealCodePointers.cs", "A9FE5E7572CA7816F9E24FE49FB051DA2B34E614914486B7A07B3C66E26B3E1E"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomLevelWord.cs", "49C2C8C13ACB4E2BA5D3C6AC304EE5E6735AF025F14F735DF6ED7500A4C6544F")],
            "Construction requires every drawable compiled collision/BTS pair. The two-input Apply derives the native command from that same pair before selecting its visual operands; callers cannot supply a mismatched command. Non-reveal pairs return null and extensions retain their compiled command without an artwork lookup. This proves drawable membership, not room overlays, traversal, metatile positions, beam geometry or pixels."),
        new("SuperMetroid.Core.Rooms.XrayOverlayVisualCatalog", "xray-complete-items-and-required-room-overlays", ["ItemMetatile", "RoomTiles"],
            [XrayCatalog, new("csharp/src/SuperMetroid.Core/Rooms/XrayOverlayRomData.cs", "AA2E43C1A69E6A3E643B779D30026FC182F83B07E000BA7253EC0342E3A1D0DD"),
             new("csharp/src/SuperMetroid.Core/Rooms/XrayRoomOverlaySourceDefinitions.cs", "C79999DAC9FCE3E71C9EB58227A08EC29C4D6DEAFDDD3F94707FCB50FDA22FC2"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "FD0E9AA1573C75DA565AA453B3FDF3C89BE9B0E5680322A539279308467C62F3"),
             new("csharp/src/SuperMetroid.Core/Rooms/CartridgeRoomHeader.cs", "456E784B457DC4B8361279DDA9069B6B039AA160EF819498EF60DF7F410A1D4B")],
            "Public construction requires eight valid item metatiles and every nonzero overlay source selected by immutable compiled room states, copying all tiles. Only item slots zero through seven and required overlay keys qualify. A semantic Core-use guard rejects the internal partial fixture factory. This proves membership, not arbitrary pointers, overlay coordinates, caller selection, traversal, host binding, placement or pixels."),
        new("SuperMetroid.Core.Rooms.RoomPlmDynamicCollectibleArtCatalog", "plm-complete-seventeen-item-uploads", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleArtCatalog.cs", "4D392128D687D27703A53B4FC86E2BE3680F235FB429A098B793738032268726"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleGraphicsDefinitions.cs", "AACC9D5CC27B974EDD9CC76C0ACB6DECCB4F2DDBBC20B08A7E740827D7D650FF"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmDynamicCollectibleGraphicsDefinitions.Generated.cs", "DF3A7FC53C7DDBB57A8CA16FD3BE6E6EE91F057A86A2C4F458D3F036D45AC782"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmSystem.Collectibles.cs", "5BD9B6917BB21DB972D7A56A6FB8DF4CB5C9F462F1D567EDA99EA4431D369971"),
             new("csharp/src/SuperMetroid.Core/Rooms/RoomPlmHeaders.cs", "77171D9ECCDF6404DB8E0C8A7F6CD273D1302165C47A9A7C73CC45C96C46E6B0")],
            "Construction rejects unknown/duplicate/null entries, requires all seventeen permanent kinds from Bombs through ReserveTank and copies only edited tile/palette payloads. Stock resolves calculated kind/source/palette definitions and the selected native artwork block directly. Resolve's required domain excludes the four fixed tank/ammo kinds. Timing, item ownership, destinations and pixel correctness are not certified."),
    ];
}
