namespace SuperMetroid.ResourceAudit;

/// <summary>Source admission and NMI routing backing the ownership comparison.</summary>
internal static class VramDmaSourceContracts
{
    // These reviewed sources connect descriptor geometry to required PNG/JSON
    // admission, importer manifests and the runtime's native/typed dispatch.
    // A changed contract is an explicit gap until its new behavior is reviewed.
    internal static readonly ReviewedSource[] Additional =
    [
        new("csharp/src/SuperMetroid.AssetExtraction/GrappleTileExtractor.cs", "541CE10437FF99168ED3E238F68ECFA3B4785231652D983961D41D2CAFEC832C"),
        new("csharp/src/SuperMetroid.AssetExtraction/BeamTileExtractor.cs", "87FDC775F27A4AC57BEEA21B9B421B5900E63EFDEA9BF14A974F04C68586E16E"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectileTrailAtlasExtractor.cs", "8978EC4D120A2E1765877B005068B0EF0FCD76CED7DF864A5C9E6F5D1CC8165E"),
        new("csharp/src/SuperMetroid.AssetExtraction/EscapeTimerTileAtlasExtractor.cs", "594CCD4179BE66C84637E008177224F0EAC17793E16FA47BF758501500EE9DFE"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusDeathTileArtworkFiles.cs", "EDE5CFA7E6D85C47C5BA60DB21C92AA2E68BDDE3CF9875339BB7104AA6FB8ABA"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusArmCannonArtworkFiles.cs", "A611907E904F0091970844C30AD0E052A51F18294E87B9981CAF415BE343A099"),
        new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "CF458EE39ACC8CB91CCC5C98D909A3E62DD5A09BD8F6FBEEF51BEE8C9664489A"),
        new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "A27811EFCBD8940CD241D7C3498E5DDD8EC39B3906CC523E93BFC4248DA58F9E"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "6CC0DC440CF5944DAB7C3B07603FA7D1D2EC032B12928E2799C2273229F101FA"),
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs", "D706327F64E76EF1904738CD6F4694B29DB8594098162908445A2D2C9FE55807"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "6A0F8917D9FB2B55391C61341DBB0E03901A2FE21AB009AFBE63D41F8E54FFDA"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlas.cs", "BC5746B3F1D6426D890E40033CDD392EA94AD807FB81115659071386094FB18B"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlasDefinitions.cs", "9E44730047C8A47B51636EA8B4598B186B2AAC2EB90A5E6CD7DDFD7FCA7CFB9B"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "1C1E490CCACF0ECC2DD5DCEC9D20531D405509461DDC875DAB7316D39A6D5133"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs", "2390C3A34C6DDA0D97FBFA05608C93A2EE1B7F08F35455F2F31A4C2E7C033C7F"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs", "5FC3060A58A9FB345A62521ED7A0DD505D7672A4EBBC8C8A0AFDED67A6268210"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs", "59020E84573C1A0470BD5B40E681B91570931F8E3DD1382B7C2934DED1694476"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileAtlas.cs", "148F7D5BBEEFB793D4184D5514B2236792C1913F9C11238190F04FBAE08509DE"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileArtworkDefinitions.cs", "CDD73487A5B7E604BE96CAC72AF1EE919802D7D038EF4F6EF271DC9BC8FBBDEB"),
        new("csharp/src/SuperMetroid.Core/Assets/TourianStatueAnimatedTileArtworkDefinitions.cs", "0D01251B2DC1141A3EB53E71D6B35CDF8A3D9181401238DDF55503E33B9F330E"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs", "76A235967E736A9B6FDC9CA2E67B26FEC0DBE71074D63F83A901608AC7E48ABA"),
        new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs", "E41E04EFE81AEB4519ACE14BFA3484D4013D7EC534061E23E0B8C3E7C6CCD43D"),
        new("csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs", "5801EDCACB25496E1EDAF78AFF5D1412EE953B2619C9C9395EC02F6478AF4E11"),
        new("csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs", "ACB555D170F035CFB2BEF171CD3A825A2C4EB7F464D54C42D72688CDD0855AD3"),
        new("csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs", "89C2E9395509B26095EF3BE583C3615BE0393E0FC03DD06253FB7701F9FD66E8"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxAnimatedTileMechanicsDefinitions.cs", "852357C1333E36F792FCC4CA02DD255CA37361C184517D790D9E83252E0F118B"),
        new("csharp/src/SuperMetroid.Core/Game/TourianStatueAnimatedTileMechanicsDefinitions.cs", "101BD6CE0ACDD08B75F8277AE806D58AAE9DD35346271A274E38AA4984343AA2"),
        new("csharp/src/SuperMetroid.Core/Game/WreckedShipTreadmillMechanicsDefinitions.cs", "333D81789E0B85435D5CF64E1AB45CC8511BD3A9AFEBC443545433B02F18A593"),
        new("csharp/src/SuperMetroid.Core/Game/ScrollingSkyChunkPointerDefinitions.cs", "87123714C3784F69DCB36F3320C1E7B923F902D641336E07A7855A1D098B7D43"),
        new("csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs", "0334F56DFEA1CB4A8E60D2754A66392782735CB13F64BE195DA3544D9809EE62"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "5852418EFBFAA78860273FC4C1DE69146BE6E871C114D4FAD382EAC188AD90E6"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomHeaderDefinitions.cs", "1257C0FAE1E000F33BEE1E8CFCC9212401AC10752FE428013585B9D4D305C8CA"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "FD0E9AA1573C75DA565AA453B3FDF3C89BE9B0E5680322A539279308467C62F3"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateSelectionDefinitions.cs", "0DEDA3C701DFF8D19280C9EB64558D8A31C55C9CE55D40B1DF541C41005D2481"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs", "A1E977737280B7E3B4DD59F9AF02CFD3EB61FD8E973C9C125B4BB5C6CEB2E28B"),
        new("csharp/src/SuperMetroid.Core/Rooms/LandingSiteEntryState.cs", "2D5613F90CD952A22FEB9336C094D7C2DEAF29BB504ACC249D424205A2628569"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectilePresentationFiles.cs", "D6E607E0DB85DB67CC84CB7DCA0668478C5662EC776F324499A41F3415D59423"),
        new("csharp/src/SuperMetroid.AssetExtraction/MapPresentationExtractor.cs", "20A8D4F32A61B9ED2DB8B07EB8F2407E3DC57182D2DC7B999A6C662B8EB5B07A"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomFxAnimatedTileAtlasExtractor.cs", "5A65977F4E08C5B95340BCCD841AFD9B2160B1603F14DC95C77525518420046E"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusBodyArtworkFiles.cs", "50DC6D9B4B8A54918E936BF4069ECDB6931B0B45D8B9985C45140731279ED185"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomSkyTilemapArtworkFiles.cs", "E42511E668408ED254DFDD028B263C2E5397BE960830DC47F0B241B639A75CDC"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.cs", "13F664C4B4EB794EF8D79EEB2CC811F6F06BCC4BAAA1A19B2E4F192F05B65404"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.Validation.cs", "89AB35FBE763ABD4D9EBE802A7C6A8B3C1777F68E6D281F136411DEF1B290E11"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameInstallation.cs", "A31B369BCEC753FFC83855718273AA9C9DF3D3875FD703D4BDA61B2D413982B8"),
    ];
    internal static void Verify(string root, VramDmaReport report)
    {
        foreach (ReviewedSource source in PlmVramArtworkSourceContract.Sources
                     .Concat(AreaMapClosedContractDefinitions.All[0].Sources).Concat(Additional).DistinctBy(source => source.Path))
            if (!ClosedPresentationAudit.MatchesReviewedSource(File.ReadAllText(Path.Combine(root, source.Path)), source))
                report.Findings.Add(new("unresolved-provider", source.Path, source.Path,
                    "Reviewed installed-artwork contract changed."));
    }
}
