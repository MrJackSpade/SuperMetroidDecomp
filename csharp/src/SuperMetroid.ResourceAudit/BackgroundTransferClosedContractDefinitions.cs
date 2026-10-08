namespace SuperMetroid.ResourceAudit;

/// <summary>Complete authored BG2/effect/upload domains, not display placement or effect mechanics.</summary>
internal static class BackgroundTransferClosedContractDefinitions
{
    private static readonly ReviewedSource SharedBg2Loader = new(
        "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameCatalog.cs", "345737A8680971A8B00A6607DF4A1D2B4B6A3FB9C2C8E1BA89E6E18FF3D4AB59");
    private static readonly ReviewedSource SharedBg2Sequence = new(
        "csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameDefinitionSequence.cs", "4CCFB8B82E6B6E4CF9D11DDBCD810ACF62078EEFED0EEA75025BEEA106DC073B");
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
            new ReviewedSource("csharp/src/SuperMetroid.Core/Assets/CrocomireBodyVisualDefinitions.cs", "7D426B07FE8D85F635157E8D85520482D670AD0C5D9483CFBAA2E92F194C2851")),
        new("SuperMetroid.Core.Assets.MotherBrainBodyBg2FrameCatalog", "mother-brain-complete-bg2-membership", ["TryGet"],
            [SharedBg2Loader, SharedBg2Layout, SharedBg2Sequence,
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainBodyBg2FrameCatalog.cs", "162F93597FECB32A9F5B6C45BA9C7EF88075316E6D49C443A23EC60C399572E7"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainBodyVisualDefinitions.cs", "18266ACCCDBBBEB15696D4934A5FD438E3E1F26573EEA34F63BDC080B1F2F531")]),
        new("SuperMetroid.Core.Assets.RoomFxLayer3TilemapCatalog", "room-fx-complete-six-pages", ["Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomFxLayer3TilemapCatalog.cs", "5E0FFFA1390B99E878D3281598BD064CE707EDCEB11D7BD4DB38E0F4F3C26388"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomFxAtmosphereTilemap.cs", "930D05F56E1EED5EBFE39A9496CD2EF4934E9091D10852293E393FC01EE90BBA"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomFxSporeTilemap.cs", "CB16C9514BAAE6CA03515F3CFD0D18FA148132AC2FDEF2462009C10037CBBC76"),
             new("csharp/src/SuperMetroid.Core/Assets/RoomFxLiquidTilemapDefinitions.cs", "1C54C0A3A730806D3DC500A0A4B35D217A06546BA6AC27E54396A936AC6DD3BF")]),
        new("SuperMetroid.Core.Assets.RoomFxPaletteBlendCatalog", "room-fx-complete-eight-blends-and-zero-clear", ["Apply", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/RoomFxPaletteBlendCatalog.cs", "E78396C5CA47FDE5A33D5B253E53C44571B789185B29E4D2F221CCBD24B66B13"),
             new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "3357C4AF41CEE8B1D02DA07845455D962C70DEEC9DF22B82978F76669C0101CD")]),
        new("SuperMetroid.Core.Assets.EndingObjectArtworkCatalog", "ending-complete-four-fragments", ["Fragment"],
            [new("csharp/src/SuperMetroid.Core/Assets/EndingObjectArtworkCatalog.cs", "D94FD0E6CF4AAA49FB58B35E2E1BC9C6AF2798A290550961F0913A06B80862D6")]),
        new("SuperMetroid.Core.Assets.GunshipLiftoffArtworkCatalog", "gunship-complete-five-takeoff-transfers", ["Resolve", "TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GunshipLiftoffArtworkCatalog.cs", "A5D9226FD3EE4FBADE070DA138D29A685CB3462920CAD725301E01936AE0D8AB"),
             new("csharp/src/SuperMetroid.Core/Hardware/IVramAssetProvider.cs", "73DCE6788B0BB9B1549CF04466AE14CB8AABA3AD9FCC2738E188ABBA2F2F8D44")]),
    ];

    private static ClosedPresentationContract Bg2(string family, string catalogHash, string definitionHash,
        params ReviewedSource[] additionalSources) => new("SuperMetroid.Core.Assets." + family + "Bg2FrameCatalog",
        family.ToLowerInvariant() + "-complete-bg2-membership", ["TryGet"],
        [SharedBg2Loader, SharedBg2Layout, SharedBg2Sequence,
         new("csharp/src/SuperMetroid.Core/Assets/" + family + "Bg2FrameCatalog.cs", catalogHash),
         new("csharp/src/SuperMetroid.Core/Assets/" + family + "Bg2FrameDefinitions.cs", definitionHash), .. additionalSources]);
}
