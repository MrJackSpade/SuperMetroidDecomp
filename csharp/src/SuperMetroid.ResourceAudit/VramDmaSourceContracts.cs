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
        new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "E2A5D6F4B5EF50E10D293A29679D4B64F3573FD140426DFB90A553ABBD67FE15"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "4942E6802A8F4B086F7C8D3F4A0FD3530E1C07B5C2F467D97CF76FE6855A1366"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "C6047AC3238CA2C39BC3739E8DAEDFDBE51767CE61966A17D5038898652D58E1"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs", "BF392FFF6867BD74D5EFCB4C9121861AF95D4A9B5E4A71962DB5F4ED1F2EAEDE"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs", "78E2DAA6A7756788C23DD272EF811D7A1EF50BC5F497E51893309C8C452BDFE4"),
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs", "F6A7375060930B0782A2E38370E943F5DA2D514E02D4E593597994F55528B6ED"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "44C416557FCF09011152213B3F15BB52517C1999175A375DA338AA293E5DED09"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlas.cs", "BC5746B3F1D6426D890E40033CDD392EA94AD807FB81115659071386094FB18B"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlasDefinitions.cs", "9E44730047C8A47B51636EA8B4598B186B2AAC2EB90A5E6CD7DDFD7FCA7CFB9B"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "1126B27483E6BD7707D0151E78885148E886554DB19B98E4DFAD215F6026B5EA"),
        new("csharp/src/SuperMetroid.Core/Game/SamusEquipmentFlags.cs", "7391925D34892CC90442E289527FB01A3D29E927DF607ACA83CF833E54C33814"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs", "D4F6243A3149BAE34E094F039B2AD2C7996CB055E00BE40AEB8FACF6A2ED66B5"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs", "E04B3B0C46A207140CFE3419E8DBF84CFCDC67316536E87C56262DF3BC9ACF39"),
             new("csharp/src/SuperMetroid.Core/Assets/SpazerCompositionGeometryDefinitions.cs", "50531F1C89026140C12B131C252FB869BACA5A6EFB931FD619376E20F20C6DCF"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyTransferDefinitions.cs", "EAEA4FC902BC3CC212C384D16FE371541373972A230EFD9A76306B4CF82C45EF"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonPlacementDefinitions.cs", "DFD26B6C54E96889B010186D0B65795BBFB9BD68621C2EF4FC379690D6AC9894"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyDefinitionLayout.cs", "AA031FE0F4ED1119B8D27B9394152B8D6FDE63516D0339C9D97D802D034B3885"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyFrameDefinitions.cs", "2DC96921E97617DFC6695A579904C86938F57C9725BE73379D426F68995D1749"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPoseDefinitions.cs", "C5FC4C4CCFD7ACFAA8833A33DF0F302CC1A6D5FC367433B05569D64B09C9942B"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs", "6A538FDB8C87B18D17F1FAC767B8C71259822D8730CFB6E2B7AB3FC8AC4AB708"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPixelDefinitions.cs", "1E9EA019CB38CA024B15EC658037EBABAB24F0CF5D43411760C2FD1EF05F34B5"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs", "5B6B3512518F81E9DBD595B5465786FD5CA903564D5E6B3C233B183B61B378DC"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapFrameDefinitions.cs", "D5EFAD79D85614FC4E6A2456329D303A154AC3AEBAD7922F4A3EE3302BC2C2E2"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs", "48F98641513B038D3B4A73EBD5D6F46F17DE8F22B74DA5D91B7B883CFD758FDE"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs", "BBB348752ABA20A9A13CD0D6E17A4A15B35E4DA199F5D9F54701A277D99C85A4"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayPrograms.cs", "4E51DF248648BBAC3C31C3C9312C4B4F2A4DF8BCF97520217C7183640547465E"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "0BFB29D5DD60BB52873474B608981D327A362CD056B39CFA7417F0236F2E61EB"),
             new("csharp/src/SuperMetroid.Core/Game/SamusMovementRomData.cs", "6AB68CB5AC06A57552E94F85625D0178987F777AE0634F1360876BFAB96F731D"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapPoseDefinitions.cs", "F4FCCF227D6D58676E8565799A8F66F74424BFD01E6FA245C0635B76C810CE0C"),
        new("csharp/src/SuperMetroid.Core/Game/SamusPoseId.cs", "2F8BDCD3C9AF53A6E0F5EB931AEFCE136B74F7B6D8D5A26FF9A1AA84AD83999A"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs", "B69DFCDA983C8E2B1178798B02FA71D278F2E8D0C95FAEF296976C0BD2189267"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboMechanicsDefinitions.cs", "09B7001995AF8DD5373C1599A837379832D0257A24CF77B5BD249678B73355B8"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboRomData.cs", "6FDDD952484A8AEB74FB025C711E27F009EDBEA0AA32D8FD6C0EC11F471A8182"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs", "9B1EB8C56D5E6A9B2DDFC6738D344980FE25FDD1F5CC0D9FBA3A162750E62813"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileAtlas.cs", "21600F3DBAE1BB3C5A82A76F124240C9119D4BF891902A96263F7CE3BEAC8AE6"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileArtworkDefinitions.cs", "1C2FCCF3BD12EF49B52D1F72810F43E73FCF25F6DBB6E02E03778EC02B5BFC81"),
        new("csharp/src/SuperMetroid.Core/Assets/TourianStatueAnimatedTileArtworkDefinitions.cs", "697D7E7EBCF798075CF6FA5A891DCD9DF805092EE349CF8847705EDFFC790B03"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs", "76A235967E736A9B6FDC9CA2E67B26FEC0DBE71074D63F83A901608AC7E48ABA"),
        new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs", "E5E588808193AAAD1B53E3659AFD7F3C4E840A766222E0B8DF121E363F8A9B5B"),
        new("csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs", "12651D69F8CE35EB1150830D97010A324CECF2FFA078E396D4DE3DBBE03ECDE4"),
        new("csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs", "95BD3E6D07D5F458F5E05D74A29E207146AE0AEA89169A7C30FE065681D78A21"),
        new("csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs", "E082F48BD639C42601B2CEDCD62E8450ECCA97D893CA014CA933D9C3F9200753"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxAnimatedTileMechanicsDefinitions.cs", "16BB1810681B3A9FC9004B5CAB52107A64C5B16DBE2BF95AC7F410DB33572973"),
        new("csharp/src/SuperMetroid.Core/Game/TourianStatueAnimatedTileMechanicsDefinitions.cs", "6DDB90542C61C0DFD432AD47D81EF39360A79DDDDFA0B0F66631B892DA3A6339"),
        new("csharp/src/SuperMetroid.Core/Game/WreckedShipTreadmillMechanicsDefinitions.cs", "FFF9E2967149B4611446A02BADDD37CB3CB531F3F60D0019D8A20310BC22953C"),
        new("csharp/src/SuperMetroid.Core/Game/ScrollingSkyChunkPointerDefinitions.cs", "87123714C3784F69DCB36F3320C1E7B923F902D641336E07A7855A1D098B7D43"),
        new("csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs", "70B6FD360923FC1A567A11175F54CFC6C46A616B9C72217A4A030E275D1E1DDB"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "B3F2088A2CBEA03FC3F2E392CDBC832109DFC1A1AFEA51C7456B20372AB4D1EC"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomHeaderDefinitions.cs", "87ECA378CC8080882D37CB2B8CB12BE06EF2D567D019E3060A53CBE98003079D"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "62A0435AE18B4C24973F36620FC79500DE554A06F73077794BDD66F3D98DFB53"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateSelectionDefinitions.cs", "1CD7DA32B77F3182C5FC314D81FF1BE30BC383AE90BEF4F11381070300C3197D"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs", "A1E977737280B7E3B4DD59F9AF02CFD3EB61FD8E973C9C125B4BB5C6CEB2E28B"),
        new("csharp/src/SuperMetroid.Core/Rooms/LandingSiteEntryState.cs", "E4A6E70B77D6DDC1D14C72AF0B9106C4519BC39475C64262B14804B85490E5DA"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectilePresentationFiles.cs", "3CFD18F30734D1EAB1D9FF1C16BA23964ABF7B63A2EF2D34B578436B6B2F2672"),
        new("csharp/src/SuperMetroid.AssetExtraction/MapPresentationExtractor.cs", "20A8D4F32A61B9ED2DB8B07EB8F2407E3DC57182D2DC7B999A6C662B8EB5B07A"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomFxAnimatedTileAtlasExtractor.cs", "5A65977F4E08C5B95340BCCD841AFD9B2160B1603F14DC95C77525518420046E"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusBodyArtworkFiles.cs", "50DC6D9B4B8A54918E936BF4069ECDB6931B0B45D8B9985C45140731279ED185"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomSkyTilemapArtworkFiles.cs", "E42511E668408ED254DFDD028B263C2E5397BE960830DC47F0B241B639A75CDC"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.cs", "DED9F4C4AA06F3BAA442F89F033E324F69BF9AC24581BB48B4D02114F6C4C2CA"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.Validation.cs", "C999B10CD13CA583F7CE8DCF3FEC87E51DDADCD9966C05EF47083F286F4D7C8A"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.Components.cs", "04F614A504B9DEAB0F4079C7C38D675BA2656FE01969D24CD9F516B8C7581D12"),
        // Format 85 requires both bounded invalid-selection beam sheets.
        new("csharp/src/SuperMetroid.AssetExtraction/GameInstallation.cs", "751883EB52229A08BE1826B7DAA371E6E974CFC96107037250FB9854950031E9"),
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
