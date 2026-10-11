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
        new("csharp/src/SuperMetroid.AssetExtraction/GrappleTileExtractor.cs", "BDA78084FE791B08AB70B3612E8A38A981E43D2CAE30C6C89AF4382AC27139BE"),
        new("csharp/src/SuperMetroid.AssetExtraction/BeamTileExtractor.cs", "DD638F6F13378DAD39D1FC3A91659FC2B6B4B756319D858013DCAF0E2DE2506F"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectileTrailAtlasExtractor.cs", "1FB4F8C24E21D92C6313DE2833F9D7E6DDCF693AC41B29931D53D3BD4D96DF70"),
        new("csharp/src/SuperMetroid.AssetExtraction/EscapeTimerTileAtlasExtractor.cs", "36FFFF0C0330B54BD107EF654355256BE4A17B19BCEC5ECE1135B6B147959711"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusDeathTileArtworkFiles.cs", "8C19B3F8367388ED016DE14074E7C20A2CDB8ED72E339C7C6A45BA3982FC70AC"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusArmCannonArtworkFiles.cs", "2BB6775D181772C0263654F9955DFD0C9F6614E04EC8E0EF39B3FC6341D900ED"),
        new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "01E5B5A246ABBC76762AEEF6C385E711F4A4574BD159F35FF3CEBFD7F64A4BCF"),
        new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "46CAAD11C6CEAAB7858AAD573FFEDEEF320370577993C861F7171EFE6FE80DFA"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "51FECBBD9AD31F43F011A5EA407A0F4398ADBC33FB1EA2797A1BC9A1B11687B5"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "18A5E04D1B6251E37A73A0BB0EC348FFF3F2548B07CB2E70F2B84573572F7067"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs", "8DCEC2C354E6F989E7DF06938BEA0A58A8771282D418E4964935F77AD0CB47B5"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs", "37C2F538981CC2AC5F7975E23FB23CDA085FBE955455B093445CD429B9030F3D"),
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs", "85E7C3E51A79FAF823B8AB20EE0D591524D7EF3F2A5C66FA0CDA03A302DE9585"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlas.cs", "F02817665B93B18573AC6148AE2C0F6A17A12DF6F91F79B6EFF7092089595D0C"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlasDefinitions.cs", "8679A8CA4C4122F210D7B5D52FC5F2F6ACCC2B65767A45EC5C3963AB95469624"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "50181E5C609316DD0F2C695663588BB09571977292EA5C829505D768E2DBDEED"),
        new("csharp/src/SuperMetroid.Core/Game/SamusEquipmentFlags.cs", "2E8852212BC9B79DA653988390705EE69A71304E11586DF73E5BC267559EE4D3"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs", "07CF7E52150B4562A559C1CA646F5E8E709B9B8F70600F1BB85B97DA19987CBD"),
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
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs", "B7A128B4A1B1FB9ABC8A89775007054698040BBE3A53F61089F4888B6C77AF97"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayPrograms.cs", "AEC295EEC2013263835DA5FCA9BE81604F6A8D3B8550CDBDCDB0999ABE335403"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "8C0C442727D4C01AE214E89B8238FAFCEBA7870AC61D1E9C13B07376AA4DF304"),
             new("csharp/src/SuperMetroid.Core/Game/SamusMovementRomData.cs", "1F0A858C191CEF505F59FBC544DA589F1481D8FE574A7795E709B9CD99204093"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapPoseDefinitions.cs", "82D88C7F296DD00736D81FBD300F77FFA928107C72F99CE40EA0D763521AF3A0"),
        new("csharp/src/SuperMetroid.Core/Game/SamusPoseId.cs", "996DFE3E185EF497B98E86164EE3A705AFD90FD2BEC63B93DA360CBC1A8ACC38"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs", "B76A58F0FD157542808E1609A8A746D0189186A4CA9BF130E9A8925DC6642A64"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboMechanicsDefinitions.cs", "C074870129A81166C9D62B619F85B4B60A5AB1F23669E8EE7B3560C181F846D8"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboRomData.cs", "6295D69960A3E5CBF3FCBEE91701B09AF80396A952BDD71FFB83FE1E8C71FC4C"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs", "CE25AACA75D7BB9EEEE09FD602F157D1A3353D3FB9EF6550BFFAF55EB1EBFC62"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileAtlas.cs", "96052F6E3A6D49F717C9EFD0155AC3317D94CB8D2D57A4235D9116432C1B1CCE"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileArtworkDefinitions.cs", "A56082B483833F2BFBE3C27E72135BAA537914A2E33B989AB4F1009D440CBCA9"),
        new("csharp/src/SuperMetroid.Core/Assets/TourianStatueAnimatedTileArtworkDefinitions.cs", "A94035B98A5BAA9A59D4710FF9ABF4904AE69E88BA7330E760636C54E79F5449"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs", "E3F623660F01CB63B35C26C82589FC4A3832460D29C7BAA9D6707B601C92A2F5"),
        new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs", "9E546C3E2FC69B26020FEDF3EAAD3186F91A3C69EE84384F419E1AE0A8D0BBC9"),
        new("csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs", "78C3985FFF2CADED7804A8F09A4887431C5751B62A71ABFA8431A83F8107C844"),
        new("csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs", "4C843F0DA75BB2AB134408B809ADCCDEC92DC669A6382A81114DC524C5A3FCD8"),
        new("csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs", "C20C54846A1C7EDFDB2745F3585EB25E5065DC2A7171234BC0C5E6CF0F29D521"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxAnimatedTileMechanicsDefinitions.cs", "0BED72D64D88AAC3D4A4B5D1959321B972C4FCBDFE5C4713A9AA645298F02ECA"),
        new("csharp/src/SuperMetroid.Core/Game/TourianStatueAnimatedTileMechanicsDefinitions.cs", "D9A79397F9EB216C57FEB802894ADE14B01621570CF94C6EB27E561093446537"),
        new("csharp/src/SuperMetroid.Core/Game/WreckedShipTreadmillMechanicsDefinitions.cs", "3FE0B99AD9E35E404BE0C86AED8C9AB28D49B20677FF0BD5F2FDDF092BEEFA61"),
        new("csharp/src/SuperMetroid.Core/Game/ScrollingSkyChunkPointerDefinitions.cs", "5225F172051E5794AAE99F0ACA578E9DD9FEE58F43F5EFD63D0767EB11341A56"),
        // #1275 re-pin: RetainSubpositions gained a word overload; no DMA source changed.
        new("csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs", "3180937AB813D51006470F48DB39ECCB27FC58F53DD35098DA45C785EC928E42"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "969BF08E3363A47517C8143B4126E553974860B1D004DDFC14F687AA8F8EE8EC"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomHeaderDefinitions.cs", "1C0D10E93213D3AB80B820744A597EC26648548F16AAC672F9C8DFE5D6A03AB0"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateDefinitions.cs", "A81815A52EF266CE17FB4EE569D9E07B1A0F67D47F0F914591E5D5FD0E92B159"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomStateSelectionDefinitions.cs", "503BDA37850F18E56E09300911C5A1E3AA1697D1F6B8CCECC1EAB63898449BF8"),
        new("csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs", "64B2BC9CCE23EFEAF9AF33233BD6615E0D75022A9F5E976F38708162315C84EF"),
        new("csharp/src/SuperMetroid.Core/Rooms/LandingSiteEntryState.cs", "A5B89C3EE88DCC364AA4CCF0A27DC03FA0ADCF4A1FA9A8B7111692904C9A520B"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectilePresentationFiles.cs", "FFDF0AF075877EC65C8534EC3F63F82181CCB7A3E2524BE8198955D6B9A93789"),
        new("csharp/src/SuperMetroid.AssetExtraction/MapPresentationExtractor.cs", "A04DAE5B38729A240B4044E26D85C2BF83F68FCDC0D5D35AA8C87A046104BA01"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomFxAnimatedTileAtlasExtractor.cs", "10F95B151DC87A25E1C658703001BA2077B58AC73F98B2FFE43ADF101EB2776D"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusBodyArtworkFiles.cs", "906731DE013507F320386C34EA6C0B2679E3CCAD2AF3D66EE7B9B3A9CABFD077"),
        new("csharp/src/SuperMetroid.AssetExtraction/RoomSkyTilemapArtworkFiles.cs", "752A4B66F549029AD172BEFDC0264C21B6B4B560A0B44B59C75C317FA7F9EEE2"),
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
