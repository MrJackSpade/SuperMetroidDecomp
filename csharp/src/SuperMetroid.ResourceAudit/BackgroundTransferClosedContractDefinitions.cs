namespace SuperMetroid.ResourceAudit;

/// <summary>Complete authored BG2/effect/upload domains, not display placement or effect mechanics.</summary>
internal static class BackgroundTransferClosedContractDefinitions
{
    private static readonly ReviewedSource SharedBg2Loader = new(
        "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameCatalog.cs", "A330767AF6BAC71E0F47A7643ADB32E1B3399168C4DB4DEEBD8736992F48B365");
    private static readonly ReviewedSource SharedBg2Sequence = new(
        "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameDefinitionSequence.cs", "DC2420942352DB6FB8E1781CDAD188A4FA9F6780401EA22C487F73E02A809AF8");
    private static readonly ReviewedSource SharedBg2Layout = new(
        "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameDefinition.cs", "8A606BC937500FC4AC942F7C33B3271C7253F3EB6AE737745190BB0CC951076F");

    internal static readonly ClosedPresentationContract[] All =
    [
        Bg2("Phantoon", "79E36732653BE385B94414E5D1FC8F40E38D108CC80EEEAB37ABC93575182CC7",
            "FA8633BC77D4FD2207F74EBB5AD0D44A02C6E04F3BF25B0399954E6EAECC887B"),
        Bg2("Draygon", "7A6607DEF60E67D7732B0FCF8DA15AC50D2C2FB4BFB9E0991DE917923BA79130",
            "A46DEE3BD5B5D6E119EE02902ED3779F2E89A1F6FC0EE2EE7B6EFA3507C51754"),
        Bg2("Crocomire", "31DA70077926433C013E54556040F416A4696DF9ED8BC35384E85B035008F4E8",
            "AEF76EA96FFA2934CF793C1EA75375E7FA428DCF13298F520F16C4E089F6B6E0",
            new ReviewedSource("csharp/src/SuperMetroid.Core/Assets/CrocomireBodyVisualDefinitions.cs", "25FAE7CDF158A29D00F6FF48B72ECE4BB47746793EB0A587DC3CC7D951C57993")),
        new("SuperMetroid.Core.Assets.MotherBrainBodyBg2FrameCatalog", "mother-brain-complete-bg2-membership", ["TryGet"],
            [SharedBg2Loader, SharedBg2Layout, SharedBg2Sequence,
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainBodyBg2FrameCatalog.cs", "621D3E4116F35C960157FF3EFA30F2D2F158F42CCB22E57B6B51A4D5D68A2215"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainBodyVisualDefinitions.cs", "F83F514B4095EAF776CE8F6080E7948E0FB302D00159CC611FB3F70D61FC43FA")]),
        new("SuperMetroid.Core.Assets.RoomFxLayer3TilemapCatalog", "room-fx-complete-six-pages", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomFxLayer3TilemapCatalog.cs", "1EBC4E4D1A15DE75212FDF1870B487BAE3E32A461FDAAC64AB2714D8475A6678"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomFxAtmosphereTilemap.cs", "44166CE0F11B56FC3EBE0FE1C26D3A737435BBE944076324904AF9B2C57AC687"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomFxSporeTilemap.cs", "0A69032E758230200FF87F52A4AE4100A7FD1415AD7EF85D51DED733EC918596"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomFxLiquidTilemapDefinitions.cs", "62DCB4A775A22BBA161A7069BD9976F1B360C84EB3038B192C7EC616B3833022")]),
        new("SuperMetroid.Core.Assets.RoomFxPaletteBlendCatalog", "room-fx-complete-eight-blends-and-zero-clear", ["Apply", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomFxPaletteBlendCatalog.cs", "C009A78F6BF1B31986339062E9DFEC3D9530C508C241A5FE4C1E00799C9D6469"),
             new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "AA01AC4CED546BD6E38C37778DBC9EDF179B42843D3B26C4651FD835B51E822D")]),
        new("SuperMetroid.Core.Assets.EndingObjectArtworkCatalog", "ending-complete-four-fragments", ["Fragment"],
            [new("csharp/src/SuperMetroid.Core/Assets/EndingObjectArtworkCatalog.cs", "12FE7DF6EA6CACB907254429A93FB7442C5A4144B28FB6B8E47E502B124F685F")]),
        new("SuperMetroid.Core.Assets.GunshipLiftoffArtworkCatalog", "gunship-complete-five-takeoff-transfers", ["Resolve", "TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GunshipLiftoffArtworkCatalog.cs", "08BC02285A0E22FA50656211D60352073AFC1A979F6396F4AB97FB1209C7FBE1"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs", "368BAF27A59AD317E14B4D907C23EA5BB2438547E593FC781712960F529E631B")]),
    ];

    private static ClosedPresentationContract Bg2(string family, string catalogHash, string definitionHash,
        params ReviewedSource[] additionalSources) => new("SuperMetroid.Core.Assets." + family + "Bg2FrameCatalog",
        family.ToLowerInvariant() + "-complete-bg2-membership", ["TryGet"],
        [SharedBg2Loader, SharedBg2Layout, SharedBg2Sequence,
         new("csharp/src/SuperMetroid.Core/Assets/" + family + "Bg2FrameCatalog.cs", catalogHash),
         new("csharp/src/SuperMetroid.Core/Assets/" + family + "Bg2FrameDefinitions.cs", definitionHash), .. additionalSources]);
}
