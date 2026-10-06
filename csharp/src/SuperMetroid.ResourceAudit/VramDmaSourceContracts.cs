namespace SuperMetroid.ResourceAudit;

/// <summary>Source admission and NMI routing backing the ownership comparison.</summary>
internal static class VramDmaSourceContracts
{
    // These reviewed sources connect descriptor geometry to required PNG/JSON
    // admission, importer manifests and the runtime's native/typed dispatch.
    // A changed contract is an explicit gap until its new behavior is reviewed.
    internal static readonly ReviewedSource[] Additional =
    [
        new("csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs", "20130DE3C3D324EC2CF06D755008385FCEB682BD30685BD0B06A1C438515CBFC"),
        new("csharp/src/SuperMetroid.AssetExtraction/GrappleTileExtractor.cs", "541CE10437FF99168ED3E238F68ECFA3B4785231652D983961D41D2CAFEC832C"),
        new("csharp/src/SuperMetroid.AssetExtraction/BeamTileExtractor.cs", "3D25366CD8BE9D65D7E381693AEB9E979DC35532B43FE954F804A7FE48BBF6E2"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectileTrailAtlasExtractor.cs", "8978EC4D120A2E1765877B005068B0EF0FCD76CED7DF864A5C9E6F5D1CC8165E"),
        new("csharp/src/SuperMetroid.AssetExtraction/EscapeTimerTileAtlasExtractor.cs", "594CCD4179BE66C84637E008177224F0EAC17793E16FA47BF758501500EE9DFE"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusDeathTileArtworkFiles.cs", "97B122DC07ABA8E804F2136BEECD6CF61CBECCD21DC7B9F6AD8A5F139A284132"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusArmCannonArtworkFiles.cs", "A611907E904F0091970844C30AD0E052A51F18294E87B9981CAF415BE343A099"),
        new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "CF458EE39ACC8CB91CCC5C98D909A3E62DD5A09BD8F6FBEEF51BEE8C9664489A"),
        new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "A27811EFCBD8940CD241D7C3498E5DDD8EC39B3906CC523E93BFC4248DA58F9E"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "8852546A00BF49F92815DBD382003DDFAF53EC723CED1108EFCD497DFB53E77B"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs", "96DE8AB068919A49D73E1EAAE74A6988A03D4B0DE7F13131F1A480EC484C2072"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs", "78E2DAA6A7756788C23DD272EF811D7A1EF50BC5F497E51893309C8C452BDFE4"),
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs", "F6A7375060930B0782A2E38370E943F5DA2D514E02D4E593597994F55528B6ED"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "F52A359EFB79D9FEA24B7D4569A4AD1467DD8A94B90CDA79CE3A7BE4654B9C74"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlas.cs", "BC5746B3F1D6426D890E40033CDD392EA94AD807FB81115659071386094FB18B"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlasDefinitions.cs", "9E44730047C8A47B51636EA8B4598B186B2AAC2EB90A5E6CD7DDFD7FCA7CFB9B"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "5772BCE58E4D673C2D2847858318E08064A5E8D7842E7D2039F3F069DCA7A985"),
        new("csharp/src/SuperMetroid.Core/Game/SamusEquipmentFlags.cs", "698EB15B595FC4192181AC1CB301F85602A2F3DAA002076CA90A37CB85A8E232"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs", "DF3E7AFF5CC7410410E8005BBE78C10C41566C0DFC4ACFFA4FD104ED2A8D091E"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs", "E04B3B0C46A207140CFE3419E8DBF84CFCDC67316536E87C56262DF3BC9ACF39"),
             new("csharp/src/SuperMetroid.Core/Assets/SpazerCompositionGeometryDefinitions.cs", "50531F1C89026140C12B131C252FB869BACA5A6EFB931FD619376E20F20C6DCF"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs", "C6C4D17762C7CE15D198E75002EFA383250973B3C2AFF62D24A42A378D772FDA"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboMechanicsDefinitions.cs", "09B7001995AF8DD5373C1599A837379832D0257A24CF77B5BD249678B73355B8"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboRomData.cs", "EA65CD34CE62929EA176D6B3A840D90DFEB8D52A94C011ADF74E8C6EF3F41767"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs", "DB2353AEFABDD5D94FE882436C3AC5AA287121FEEE8A152C31155B6612CB9028"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileAtlas.cs", "21600F3DBAE1BB3C5A82A76F124240C9119D4BF891902A96263F7CE3BEAC8AE6"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileArtworkDefinitions.cs", "1C2FCCF3BD12EF49B52D1F72810F43E73FCF25F6DBB6E02E03778EC02B5BFC81"),
        new("csharp/src/SuperMetroid.Core/Assets/TourianStatueAnimatedTileArtworkDefinitions.cs", "697D7E7EBCF798075CF6FA5A891DCD9DF805092EE349CF8847705EDFFC790B03"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs", "76A235967E736A9B6FDC9CA2E67B26FEC0DBE71074D63F83A901608AC7E48ABA"),
        new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs", "37E6C6EF38C71A358473389370DF9B810D79A910BE5D7B182296D701908E4DBB"),
        new("csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs", "504DF969C16A79B64C412FF902479E305893CC70AB286F6997BB28A337929E7B"),
        new("csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs", "7998F249FD22F4939CF588EA62648C56B7C0BFDDE3571D8286103B64D607DF7E"),
        new("csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs", "89C2E9395509B26095EF3BE583C3615BE0393E0FC03DD06253FB7701F9FD66E8"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxAnimatedTileMechanicsDefinitions.cs", "D092E82F7F2117230C4D7CD9873BD74BE96BB4F55CAF4E0C26BB25FA4452B277"),
        new("csharp/src/SuperMetroid.Core/Game/TourianStatueAnimatedTileMechanicsDefinitions.cs", "6DDB90542C61C0DFD432AD47D81EF39360A79DDDDFA0B0F66631B892DA3A6339"),
        new("csharp/src/SuperMetroid.Core/Game/WreckedShipTreadmillMechanicsDefinitions.cs", "5F106D9E3636EE059AA5C223A00CE6332B8C9E122B8587CB4218265759291E79"),
        new("csharp/src/SuperMetroid.Core/Game/ScrollingSkyChunkPointerDefinitions.cs", "87123714C3784F69DCB36F3320C1E7B923F902D641336E07A7855A1D098B7D43"),
        new("csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs", "3EB47A67CAEDAD19C2BCBFD46DB9CEE6C6611FAFAB3B77DAD8B5E51DA5A86B7B"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "EEAAAAE46BA3D0FC53EDD5A4F9E429D21FE361CA22B375636936519EA4E98A21"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomHeaderDefinitions.cs", "1257C0FAE1E000F33BEE1E8CFCC9212401AC10752FE428013585B9D4D305C8CA"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "FD0E9AA1573C75DA565AA453B3FDF3C89BE9B0E5680322A539279308467C62F3"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateSelectionDefinitions.cs", "0DEDA3C701DFF8D19280C9EB64558D8A31C55C9CE55D40B1DF541C41005D2481"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs", "A1E977737280B7E3B4DD59F9AF02CFD3EB61FD8E973C9C125B4BB5C6CEB2E28B"),
        new("csharp/src/SuperMetroid.Core/Rooms/LandingSiteEntryState.cs", "2D5613F90CD952A22FEB9336C094D7C2DEAF29BB504ACC249D424205A2628569"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectilePresentationFiles.cs", "3CFD18F30734D1EAB1D9FF1C16BA23964ABF7B63A2EF2D34B578436B6B2F2672"),
        new("csharp/src/SuperMetroid.AssetExtraction/MapPresentationExtractor.cs", "20A8D4F32A61B9ED2DB8B07EB8F2407E3DC57182D2DC7B999A6C662B8EB5B07A"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomFxAnimatedTileAtlasExtractor.cs", "5A65977F4E08C5B95340BCCD841AFD9B2160B1603F14DC95C77525518420046E"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusBodyArtworkFiles.cs", "50DC6D9B4B8A54918E936BF4069ECDB6931B0B45D8B9985C45140731279ED185"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomSkyTilemapArtworkFiles.cs", "E42511E668408ED254DFDD028B263C2E5397BE960830DC47F0B241B639A75CDC"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.cs", "13F664C4B4EB794EF8D79EEB2CC811F6F06BCC4BAAA1A19B2E4F192F05B65404"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.Validation.cs", "89AB35FBE763ABD4D9EBE802A7C6A8B3C1777F68E6D281F136411DEF1B290E11"),
        // Format 85 requires both bounded invalid-selection beam sheets.
        new("csharp/src/SuperMetroid.AssetExtraction/GameInstallation.cs", "76FA1E22C388FA04012A4EB77F281A10F81CE7EB2C34B272F932E8A86A9A3DE3"),
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
