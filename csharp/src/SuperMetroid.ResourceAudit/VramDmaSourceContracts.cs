namespace SuperMetroid.ResourceAudit;

/// <summary>Source admission and NMI routing backing the ownership comparison.</summary>
internal static class VramDmaSourceContracts
{
    // These reviewed sources connect descriptor geometry to required PNG/JSON
    // admission, importer manifests and the runtime's native/typed dispatch.
    // A changed contract is an explicit gap until its new behavior is reviewed.
    internal static readonly ReviewedSource[] Additional =
    [
        new("csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs", "6771012D37AA78FCB0E4439A9F9756ABE23F4C8A8C319D8D9A4C4FBBD9E2924C"),
        new("csharp/src/SuperMetroid.AssetExtraction/GrappleTileExtractor.cs", "CEBA7CCC89D30266D369DA4CA42AC0C0115FF7009DF9C30E4C7646BBA68493A7"),
        new("csharp/src/SuperMetroid.AssetExtraction/BeamTileExtractor.cs", "632D2633A5E3610B242C56BDDDE0271354869C78EC43192D88490B9123A9E6E9"),
        new("csharp/src/SuperMetroid.AssetExtraction/ProjectileTrailAtlasExtractor.cs", "5E6BE84E137CD48D83BE01B03834CA1586F53731B8BF0CD161D6EB83AC6583A8"),
        new("csharp/src/SuperMetroid.AssetExtraction/EscapeTimerTileAtlasExtractor.cs", "1F034B3CF75994953276E76AF78EE97C54269FEBD985D774252678F2C142DFB2"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusDeathTileArtworkFiles.cs", "273E8A7A5D8D6355218FFA854C8E588F483413296765431BC59460A946BF611C"),
        new("csharp/src/SuperMetroid.AssetExtraction/SamusArmCannonArtworkFiles.cs", "A84B98B819BA79F620FECE116EFBC83AFA19180713721B4B8FAFEF1DF0DF4785"),
        new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs", "7E13642A0E1B95613BC2F4B1C51082711C43BBDCAF0A4B7EE58F686280E9E871"),
        new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs", "18E30F613BFAD79AB0C4B8093D1041749062C6BFFA57B347C69138FED38BAD21"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs", "51FECBBD9AD31F43F011A5EA407A0F4398ADBC33FB1EA2797A1BC9A1B11687B5"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs", "49C7E5915AEFAA06AAC4FE3F2BC702B7246DCBADA645765686F9F7AF3D83510F"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs", "DC076C56917029A93D0D97885AB7A5E0344E7B9C9EC02276C18C63183DF3AA15"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs", "BD5C4C72DCD287EB066548250C87C85CF59F8D467B694FEE0D7595E682A69B8A"),
        new("csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.ProjectilePresentation.cs", "4986C80369CFA277ACEA3B224B7CBB88E31A7DA8BDEFEF2ADA6C53D32000FD9D"),
        new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs", "9E3C7A3D71A2F861F3DA0F9C09DF2A8F9BAED7B5BFA6DAA0064F53C2624A4892"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlas.cs", "83FB939DF5E2C951E9587876FBD77823D6AA3BAAF0049E1B093E4237A7E305D0"),
        new("csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlasDefinitions.cs", "8679A8CA4C4122F210D7B5D52FC5F2F6ACCC2B65767A45EC5C3963AB95469624"),
        new("csharp/src/SuperMetroid.Core/Assets/BeamTileCatalog.cs", "8E1F84FC08BB7FD11FB5E6D9797E57CA52C59CE26D5E862F15FE746ED4E44540"),
        new("csharp/src/SuperMetroid.Core/Game/SamusEquipmentFlags.cs", "2E8852212BC9B79DA653988390705EE69A71304E11586DF73E5BC267559EE4D3"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs", "17E631AFD63678A8E007341981ECF0581091AD585C72BD32F398E8415329FE7C"),
             new("csharp/src/SuperMetroid.Core/Assets/BeamTileAtlasDefinitions.cs", "E613D428D1E3D44B7669C0053D29683A849CAA0DDDF383EAA4A2C10C4B37778B"),
             new("csharp/src/SuperMetroid.Core/Assets/SpazerCompositionGeometryDefinitions.cs", "1E118EEDF3057E8FD81E575E3133003AB51C9B1D84D1912B198C82EAFC691FF8"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyTransferDefinitions.cs", "45CD6CD6EE78F55F57674E3409410C931B585412D6B9AC96DD2AE394AB663D81"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonPlacementDefinitions.cs", "4B0B9976585AB8D26F7F6E7A08F3F573B8CE08CFA7E67D5837EF8FD9BE6D658A"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyDefinitionLayout.cs", "21700346BC5B9F4AE863364301BF30BE52C32F171E0815E1D394E917AE00417A"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyFrameDefinitions.cs", "D10EFAF9C8EC04F6878D8BA9DD740046BAE8CFE07601422BD5A388EC586106B3"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPoseDefinitions.cs", "CC7B22A530795D32AA66349CDD8DF6F81F1D92FC4C1F5974B497288952227A8F"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs", "9299F425F0B0C95D64AA010359B4E9A10AB6F2C57F2C62A932A3B7FECBDE25D4"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusBodyPixelDefinitions.cs", "836BE1CAB9EA0038E60CCEFC22BBDE1A86A0165DEFB0E2E140133082E5051D03"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs", "A3EBC9C3919859C3467276DAF51D07B5E990E38354A305B6563ED0101627AD71"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapFrameDefinitions.cs", "18BE0A49027E590EAF276E5D8CCF043D3E92AA1FEB523E9B8B841715B9AD316F"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs", "7BF84FD6D6B3185B670FC2DB0C8951B53D886A8B4547080C35DC6C7E909B0849"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs", "F89C93F4EEA1EF4FE5CA88BEB71BA51B7B52549C5EF3E79CEFD41E63A8CD2CFF"),
             new("csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayPrograms.cs", "9BCF1BD6BBAC548A8680F964FD043C416F569DB2ED2460F3E792DC0906D4AC52"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "8C0C442727D4C01AE214E89B8238FAFCEBA7870AC61D1E9C13B07376AA4DF304"),
             new("csharp/src/SuperMetroid.Core/Game/SamusMovementRomData.cs", "EE9D40C31A450BAFF376746F319B29B63D0B15308A6CBD85DCE3861DDB263247"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusSpritemapPoseDefinitions.cs", "E777F900E1075C08E1ACF2921E38547ECDC81151C3BF24559B5BDDD0FE0D0DD4"),
        new("csharp/src/SuperMetroid.Core/Game/SamusPoseId.cs", "A3807AAA45D8EF8F1454F35F6EA8395A5A8F7A99A2274B442B29988CBD1F2E13"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs", "DBF1D6FBB49EA9A0676E332FA8E44181B43F093A9FCCBF3973CB6B8B7C1EB08D"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboMechanicsDefinitions.cs", "4FAC457BD428CECDCB880BD90C2258B016E6FF0E39BD7793B56BB2F1BA250E11"),
             new("csharp/src/SuperMetroid.Core/Game/SamusComboRomData.cs", "6295D69960A3E5CBF3FCBEE91701B09AF80396A952BDD71FFB83FE1E8C71FC4C"),
        new("csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs", "C494410ED50A09F267D07CB80F250FA2BB7878AA845CF427B0F292289A03A45E"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileAtlas.cs", "D215952ADE4E51862C7F6BD5CBA09DAA88FFCB8C165BACA1F518DE473B87C203"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileArtworkDefinitions.cs", "98ABCE140C2C9C3E37B0BCDFAB9F5DE31379D5A126B8A57CA12D69FF76404C6E"),
        new("csharp/src/SuperMetroid.Core/Assets/TourianStatueAnimatedTileArtworkDefinitions.cs", "B7C7CF6FECF43DA776C2AD205A08A4C429871FF004928693210BAB72C50D0B33"),
        new("csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs", "E3F623660F01CB63B35C26C82589FC4A3832460D29C7BAA9D6707B601C92A2F5"),
        new("csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs", "9E546C3E2FC69B26020FEDF3EAAD3186F91A3C69EE84384F419E1AE0A8D0BBC9"),
        new("csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs", "486A2FE7ED4E39832B490081EEFED79EB36AB4F1FA6577F913958AA0D30D0FB2"),
        new("csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs", "4C843F0DA75BB2AB134408B809ADCCDEC92DC669A6382A81114DC524C5A3FCD8"),
        new("csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs", "898670E7E4C91841889ED94ABC75359A1CB9B1712D8F1C8672B98DD2A7D010E6"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxAnimatedTileMechanicsDefinitions.cs", "2567A18836A07341333FBD7AD7C60C0BEF691CCFB3467644B6999D2D74BB4342"),
        new("csharp/src/SuperMetroid.Core/Game/TourianStatueAnimatedTileMechanicsDefinitions.cs", "391DCB7682DB5199CAC1C37EA1DE9DE8FF1C1025EEF9E30CF2EAAD64DA057DFE"),
        new("csharp/src/SuperMetroid.Core/Game/WreckedShipTreadmillMechanicsDefinitions.cs", "4A7E4B22DBCB64DD115D35E0F3A0EBF99F26F1AAC3AE4ECC9C425BA4E8C212D4"),
        new("csharp/src/SuperMetroid.Core/Game/ScrollingSkyChunkPointerDefinitions.cs", "8747695CE2D92911FA95462077D2D06877BBA8FD482B41BF4A7EC3783AB94A08"),
        // #1275 re-pin: RetainSubpositions gained a word overload; no DMA source changed.
        new("csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs", "FFEDE365E186A7610A0A5507EDD9D346B2DA5603D181EC2EF869CA63AF846EA5"),
        new("csharp/src/SuperMetroid.Core/Game/RoomFxRomData.cs", "AA01AC4CED546BD6E38C37778DBC9EDF179B42843D3B26C4651FD835B51E822D"),
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
        new("csharp/src/SuperMetroid.AssetExtraction/GameAssetInstaller.cs", "C18C2C1DE9A86CF1EDEB27F0F3FC67D416D64D2255ABA41213CDB8FECF05D2AE"),
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
