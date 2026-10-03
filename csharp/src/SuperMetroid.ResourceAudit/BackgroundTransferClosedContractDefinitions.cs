namespace SuperMetroid.ResourceAudit;

/// <summary>Complete authored BG2/effect/upload domains, not display placement or effect mechanics.</summary>
internal static class BackgroundTransferClosedContractDefinitions
{
    private static readonly ReviewedSource SharedBg2Loader = new(
        "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameCatalog.cs", "345737A8680971A8B00A6607DF4A1D2B4B6A3FB9C2C8E1BA89E6E18FF3D4AB59");
    private static readonly ReviewedSource SharedBg2Sequence = new(
        "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameDefinitionSequence.cs", "79FAE6EA9A4065E9E534B417A9544276302563526B51102641BC94BA9918D4A0");
    private static readonly ReviewedSource SharedBg2Layout = new(
        "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameDefinition.cs", "3F8202C79159289840C9E36C40B3B4F4B02A355582641236A79C4ACFD0321F66");

    internal static readonly ClosedPresentationContract[] All =
    [
        Bg2("Phantoon", "A9CDCD7E43E6B958A053780E2CECF5FF2BE13553149BC81648B9CD9CCB3F99B9",
            "E349DF3C0E8D4A5CD43FA3741000A7FB6F182C2006146867B56CF8FBD4583911"),
        Bg2("Draygon", "30039A698A1284957CB7BAA1664381CE6F3210D46021F6FBAF4B1732B73F0964",
            "E84E0DEA060122819331D762D81224A6DB9BBA75D292A3163ABB70A938BC17EE"),
        Bg2("Crocomire", "ACADDFD75B4055235FFAF8AD6D8F668DA96244861CE63CBAA72C4DFD73EDC96C",
            "68426254F61EB8595D0C8C76BF19AAA9D0F4BB200BBBD94A75C92F3948440B7D",
            new ReviewedSource("csharp/src/SuperMetroid.Core/Assets/CrocomireBodyVisualDefinitions.cs", "465B7A04AB313EF7272DA2644089243D3D30EBC46D2A522A9D8A8C535BC92B34")),
        new("SuperMetroid.Core.Assets.MotherBrainBodyBg2FrameCatalog", "mother-brain-complete-bg2-membership", ["TryGet"],
            [SharedBg2Loader, SharedBg2Layout, SharedBg2Sequence,
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainBodyBg2FrameCatalog.cs", "162F93597FECB32A9F5B6C45BA9C7EF88075316E6D49C443A23EC60C399572E7"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainBodyVisualDefinitions.cs", "272720D783F2800070CD7C224997A465C52DD765BCC6569E0438316E1244658B")],
            "Private construction requires all sixteen declared mixed-body BG2 frames with independently compiled bounded writes. The OAM-only dummy and other unowned pointers validly return false. This is resource membership, not body composition, scroll, positioning, collision or battle parity."),
        new("SuperMetroid.Core.Assets.RoomFxLayer3TilemapCatalog", "room-fx-complete-six-pages", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomFxLayer3TilemapCatalog.cs", "DDDC51C927BDB239A10BFC35BE657E3BC80FFB12542C4A44824392CEAEE47D88"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomFxLiquidTilemapDefinitions.cs", "1C54C0A3A730806D3DC500A0A4B35D217A06546BA6AC27E54396A936AC6DD3BF")],
            "Private Load requires every one of the six named 32x33 effect pages and independently compiles each cell. Only those six effect types select a resource. Liquid height, timing, HDMA, source selection, VRAM upload and pixels are not certified."),
        new("SuperMetroid.Core.Assets.RoomFxPaletteBlendCatalog", "room-fx-complete-eight-blends-and-zero-clear", ["Apply", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomFxPaletteBlendCatalog.cs", "E03F9349FFD1A46E2BD1664874B9BB4DF9D7F758305C923679B7AC2B58AC94AD"),
             new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "EEAAAAE46BA3D0FC53EDD5A4F9E429D21FE361CA22B375636936519EA4E98A21")],
            "Private Load requires all eight sparse three-color blend selections and independently validates/compiles their colors. Apply(0) is a compiled clear of color 27, not a ninth resource; Resolve(0) remains invalid. This does not certify the caller's chosen FX, color math or haze appearance."),
        new("SuperMetroid.Core.Assets.EndingObjectArtworkCatalog", "ending-complete-four-fragments", ["Fragment"],
            [new("csharp/src/SuperMetroid.Core/Assets/EndingObjectArtworkCatalog.cs", "D94FD0E6CF4AAA49FB58B35E2E1BC9C6AF2798A290550961F0913A06B80862D6")],
            "Public construction requires and independently copies all four nonnull explosion fragments of the exact native transfer length. Fragment bounds-checks the four exclusive IDs. This does not certify other ending properties, clocks, scene ordering, transparency, upload destinations or pixels."),
        new("SuperMetroid.Core.Assets.GunshipLiftoffArtworkCatalog", "gunship-complete-five-takeoff-transfers", ["Resolve", "TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GunshipLiftoffArtworkCatalog.cs", "A5D9226FD3EE4FBADE070DA138D29A685CB3462920CAD725301E01936AE0D8AB"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs", "D60B0DED6A14D23F5962FD93DA6AF5B48D83524D344A3ABB31548334EA2492E4")],
            "Construction requires all five 1024-byte takeoff atlases and clones the frame array. Typed Resolve selects only those five asset IDs; legacy TryResolve validates the exact byte count for owned sources and returns false for others. Caller handoff, destinations, takeoff motion and pixels are not certified."),
    ];

    private static ClosedPresentationContract Bg2(string family, string catalogHash, string definitionHash,
        params ReviewedSource[] additionalSources) => new("SuperMetroid.Core.Assets." + family + "Bg2FrameCatalog",
        family.ToLowerInvariant() + "-complete-bg2-membership", ["TryGet"],
        [SharedBg2Loader, SharedBg2Layout, SharedBg2Sequence,
         new("csharp/src/SuperMetroid.Core/Assets/" + family + "Bg2FrameCatalog.cs", catalogHash),
         new("csharp/src/SuperMetroid.Core/Assets/" + family + "Bg2FrameDefinitions.cs", definitionHash), .. additionalSources],
        "The reviewed private wrapper requires every declared family BG2 frame through the exact-set shared loader. " +
        "Each ordered write is independently compiled and bounds-checked. Unowned pointers validly return false. " +
        "This proves membership only, not selected native operands, OAM/terrain layering, placement or timing.");
}
