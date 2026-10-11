namespace SuperMetroid.ResourceAudit;

/// <summary>Source admission and NMI routing backing the ownership comparison.</summary>
internal static class VramDmaSourceContracts
{
    // These reviewed sources connect descriptor geometry to required PNG/JSON
    // admission, importer manifests and the runtime's native/typed dispatch.
    // A changed contract is an explicit gap until its new behavior is reviewed.
    internal static readonly ReviewedSource[] Additional =
    [
        new("csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs", "33ABD90314ACAFCEFE007360A5DB5D4BC9225D4AFFD2EE081F4338F32D368660"),
        new("csharp/src/SuperMetroid.AssetExtraction/GrappleTileExtractor.cs", "CEBA7CCC89D30266D369DA4CA42AC0C0115FF7009DF9C30E4C7646BBA68493A7"),
        new("csharp/src/SuperMetroid.AssetExtraction/BeamTileExtractor.cs", "F06E0EE116B7243FC61C4B4F75E13ED5D84641D4E52DAADC6E3C682FFC17FB5C"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectileTrailAtlasExtractor.cs", "5E6BE84E137CD48D83BE01B03834CA1586F53731B8BF0CD161D6EB83AC6583A8"),
        new("csharp/src/SuperMetroid.AssetExtraction/EscapeTimerTileAtlasExtractor.cs", "1F034B3CF75994953276E76AF78EE97C54269FEBD985D774252678F2C142DFB2"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusDeathTileArtworkFiles.cs", "273E8A7A5D8D6355218FFA854C8E588F483413296765431BC59460A946BF611C"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusArmCannonArtworkFiles.cs", "A84B98B819BA79F620FECE116EFBC83AFA19180713721B4B8FAFEF1DF0DF4785"),
        new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "7E13642A0E1B95613BC2F4B1C51082711C43BBDCAF0A4B7EE58F686280E9E871"),
        new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "535C19B6CD3747971C1CAD94F70A759F1F37F2A167CE6D874DFC35C76F24E74A"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "51FECBBD9AD31F43F011A5EA407A0F4398ADBC33FB1EA2797A1BC9A1B11687B5"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "F34C6A5CEB68F126CF2E4A1C01D2189FC0F4FF84CEEEB47B3358077A296DD3FD"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs", "176CFCBF127A3F3F41F0E2E81F81F522323FCAB8B334F07162464D7E62E81A0E"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs", "BD5C4C72DCD287EB066548250C87C85CF59F8D467B694FEE0D7595E682A69B8A"),
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs", "85E7C3E51A79FAF823B8AB20EE0D591524D7EF3F2A5C66FA0CDA03A302DE9585"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlas.cs", "83FB939DF5E2C951E9587876FBD77823D6AA3BAAF0049E1B093E4237A7E305D0"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlasDefinitions.cs", "8679A8CA4C4122F210D7B5D52FC5F2F6ACCC2B65767A45EC5C3963AB95469624"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "50181E5C609316DD0F2C695663588BB09571977292EA5C829505D768E2DBDEED"),
        new("csharp/src/SuperMetroid.Core/Game/SamusEquipmentFlags.cs", "2E8852212BC9B79DA653988390705EE69A71304E11586DF73E5BC267559EE4D3"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs", "CD91339A6E96694945656D5DDE6EE61350BCBD91AB74BFB20CD93C9CC6102B9B"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs", "DDFAAF3A4D7581D3C3238C8444ADCD45EC6E1FFF350390A5EE87C365DE41416D"),
             new("csharp/src/SuperMetroid.Core/Assets/SpazerCompositionGeometryDefinitions.cs", "1E118EEDF3057E8FD81E575E3133003AB51C9B1D84D1912B198C82EAFC691FF8"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyTransferDefinitions.cs", "44FE41A6F5B2C05B8F372C9204C8116F60F984B199A4E6835C83F703BBFB7D2A"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonPlacementDefinitions.cs", "4BA7312382B06BF65CF7F09D96CB9947E58EEBA63E3A01CD68D748E8CECE6804"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyDefinitionLayout.cs", "21700346BC5B9F4AE863364301BF30BE52C32F171E0815E1D394E917AE00417A"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyFrameDefinitions.cs", "2D75B9A6939185909469C37CF1711ECCA5BA59B1FA1CC137E81D4A88A8168AF3"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPoseDefinitions.cs", "EBEF3087A9892090B00AB17D5FBC0827B40D6D4344551B60A52C40561A00D184"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs", "109C60BC93864BEA7F6E0AD7D8424A037D69F3DE67570F3D0AA1E110D967F3DE"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPixelDefinitions.cs", "836BE1CAB9EA0038E60CCEFC22BBDE1A86A0165DEFB0E2E140133082E5051D03"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs", "B0EF6EB7D1154ADDE345A678959A5FCC97EB94E3555B2633864B1DB7C24B3445"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapFrameDefinitions.cs", "3513BD799155CE8428C97652B5FFF68A8B1CC5175AF40958742BF6D1C6054B54"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs", "7BF84FD6D6B3185B670FC2DB0C8951B53D886A8B4547080C35DC6C7E909B0849"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs", "508F338277A39A05D1257D29F9E38F03AC902F004543FBEC1E0075DE4E3D9DDD"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayPrograms.cs", "AEC295EEC2013263835DA5FCA9BE81604F6A8D3B8550CDBDCDB0999ABE335403"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "8C0C442727D4C01AE214E89B8238FAFCEBA7870AC61D1E9C13B07376AA4DF304"),
             new("csharp/src/SuperMetroid.Core/Game/SamusMovementRomData.cs", "1F0A858C191CEF505F59FBC544DA589F1481D8FE574A7795E709B9CD99204093"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapPoseDefinitions.cs", "82D88C7F296DD00736D81FBD300F77FFA928107C72F99CE40EA0D763521AF3A0"),
        new("csharp/src/SuperMetroid.Core/Game/SamusPoseId.cs", "996DFE3E185EF497B98E86164EE3A705AFD90FD2BEC63B93DA360CBC1A8ACC38"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs", "B76A58F0FD157542808E1609A8A746D0189186A4CA9BF130E9A8925DC6642A64"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboMechanicsDefinitions.cs", "C074870129A81166C9D62B619F85B4B60A5AB1F23669E8EE7B3560C181F846D8"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboRomData.cs", "6295D69960A3E5CBF3FCBEE91701B09AF80396A952BDD71FFB83FE1E8C71FC4C"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs", "2146A35A170015C988A301562FB3A63FD88ED90C84B1D8EF1D1AB8984ADFDD5C"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileAtlas.cs", "E5C9D2C59C52987AA9112392F541DAF5DCF62E9503287847E64B3E9E03DB344E"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileArtworkDefinitions.cs", "A56082B483833F2BFBE3C27E72135BAA537914A2E33B989AB4F1009D440CBCA9"),
        new("csharp/src/SuperMetroid.Core/Assets/TourianStatueAnimatedTileArtworkDefinitions.cs", "A94035B98A5BAA9A59D4710FF9ABF4904AE69E88BA7330E760636C54E79F5449"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs", "E3F623660F01CB63B35C26C82589FC4A3832460D29C7BAA9D6707B601C92A2F5"),
        new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs", "9E546C3E2FC69B26020FEDF3EAAD3186F91A3C69EE84384F419E1AE0A8D0BBC9"),
        new("csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs", "78C3985FFF2CADED7804A8F09A4887431C5751B62A71ABFA8431A83F8107C844"),
        new("csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs", "4C843F0DA75BB2AB134408B809ADCCDEC92DC669A6382A81114DC524C5A3FCD8"),
        new("csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs", "8BD5A78D074B5B07B48CAE003974C0930D721C22382835DF039A50B10DB87CC4"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxAnimatedTileMechanicsDefinitions.cs", "0BED72D64D88AAC3D4A4B5D1959321B972C4FCBDFE5C4713A9AA645298F02ECA"),
        new("csharp/src/SuperMetroid.Core/Game/TourianStatueAnimatedTileMechanicsDefinitions.cs", "D9A79397F9EB216C57FEB802894ADE14B01621570CF94C6EB27E561093446537"),
        new("csharp/src/SuperMetroid.Core/Game/WreckedShipTreadmillMechanicsDefinitions.cs", "3FE0B99AD9E35E404BE0C86AED8C9AB28D49B20677FF0BD5F2FDDF092BEEFA61"),
        new("csharp/src/SuperMetroid.Core/Game/ScrollingSkyChunkPointerDefinitions.cs", "5225F172051E5794AAE99F0ACA578E9DD9FEE58F43F5EFD63D0767EB11341A56"),
        // #1275 re-pin: RetainSubpositions gained a word overload; no DMA source changed.
        new("csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs", "729BEC534A07C2649F632BBCF04DC63E69C73A4909DA2FBB809697F080063273"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "969BF08E3363A47517C8143B4126E553974860B1D004DDFC14F687AA8F8EE8EC"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomHeaderDefinitions.cs", "CE8FD8936BC72CCEE400695CEAE8BD670D766371A3FB59BA9F13D36D68564F3B"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "93A78AC57BDA8AB714CA6D250C9A3CA7008EF1F254D2047BA4277ACB1A9AEFD6"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateSelectionDefinitions.cs", "503BDA37850F18E56E09300911C5A1E3AA1697D1F6B8CCECC1EAB63898449BF8"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs", "E62E55CE4FD9C35F31303FA0042E7674ED8A9587B5B61244B031AB0A92D42159"),
        new("csharp/src/SuperMetroid.Core/Rooms/LandingSiteEntryState.cs", "83953AD1667BEE3CBFB5D8D23B3F955E16C55F4BEB709B25DBDACAECC3150298"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectilePresentationFiles.cs", "FFDF0AF075877EC65C8534EC3F63F82181CCB7A3E2524BE8198955D6B9A93789"),
        new("csharp/src/SuperMetroid.AssetExtraction/MapPresentationExtractor.cs", "27FDFE18974F72DAE4FB9E894DD4A9F84ADF9ADBA4D7337A13887079A0E1E5CB"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomFxAnimatedTileAtlasExtractor.cs", "E96C535DB461D58D2805A03CA2CEB2106052517C3BEB2442FF2CF1867C89DFD1"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusBodyArtworkFiles.cs", "0B75E09D8348F6E50FC26057BD32B889C978C5F6151B031C65AAA3091AB3A19C"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomSkyTilemapArtworkFiles.cs", "7B3F2B5BD6F5FD4A672AC472DFB463EAC7A30600F5C1E23298BD0F45BB19FB1D"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.cs", "4C17CD805728C12FFDAAAAA932D0631A903E28433457A488C78DC8D26903DEA9"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.Validation.cs", "D603B571267F754664C1A001B1F8A2D1ABF9454A38C1C430FCCA8CCF0EB9E492"),
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.Components.cs", "8B7456A523E142C690DEE4BA9791AC95B9EC9994012B9A446928128673D10556"),
        // Format 85 requires both bounded invalid-selection beam sheets.
        new("csharp/src/SuperMetroid.AssetExtraction/GameInstallation.cs", "1D95CBDEA7D3070436D84CE7E12AE9DDFAA52796689DFD580FC18CFD1C46D14F"),
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
