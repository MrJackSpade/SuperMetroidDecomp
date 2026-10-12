namespace SuperMetroid.ResourceAudit;

/// <summary>Complete imported color sequences; no battle, death or palette animation is executed.</summary>
internal static class SequenceColorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.MotherBrainRainbowPalettePresentation", "mother-brain-rainbow-v3-complete-sequences",
            ["TryReadBeamColor", "ApplyRainbow", "ApplyNormal", "ApplyToGrey", "ApplyFromGrey",
                "ApplyFakeDeathToGrey", "ApplyFakeDeathFromGrey"],
            // #629 re-pinned: the grey trailing word is written through MotherBrainPaletteMemory and the WRAM address constants moved to catalogs; colors unchanged.
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs", "51A25CD72942228D2FC88B02B2A58C6DB9384F9E062B348486C3C2BA33D388B7"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowShadeChannel.cs", "E313E42DCD0CEF8F940E1DEB358B292BE3DBAA09244190E82CA18140B53C1CA4"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "90B3B500714734717CC515A558FC4A16BB382AC45300BA5AF62FD5B8FB963084"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "EF4F5F9E2463C12A6C3F37BE9B190186F354C93DF3CED57F1120773C83736612"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDrainedPaintDefinitions.cs", "C56644A66B8C5BC1DF28D7892DFB58CBFBEA4F87C7EDC729317D47414AD7E6AB"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowPaletteRomData.cs", "5093BEDF978F606D0E670F90DCFF088D0926FFE306C7A7168A72F2BB216403AA"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDrainedPaletteRomData.cs", "D630C3C47CB26913C4D5B284059EB8798708F7412D465A33683B686DCC853C4E"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainFakeDeathPaletteRomData.cs", "F1F55DBEA104BE97D296D42F1A302230CC1C2CBF41E5A25DC9EAAAA606811559"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainBeamRomData.cs", "4096A6180390B972682800CD645297F5996FA4FB15A92D140572F1FDFA28E50F")]),
        new("SuperMetroid.Core.Assets.MotherBrainDeathColorCatalog", "mother-brain-death-v1-complete-color-rows",
            ["BodyColor", "LegColor", "CorpseColor", "ExplodedDoorColor"],
            [new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs", "53EFF3A2A7273D7AA872F1EDB55DD4541B32925B8CD54E20750053FFB752D662"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs", "90B3B500714734717CC515A558FC4A16BB382AC45300BA5AF62FD5B8FB963084"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPaintDefinitions.cs", "EF4F5F9E2463C12A6C3F37BE9B190186F354C93DF3CED57F1120773C83736612"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainDrainedPaintDefinitions.cs", "C56644A66B8C5BC1DF28D7892DFB58CBFBEA4F87C7EDC729317D47414AD7E6AB"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs", "51A25CD72942228D2FC88B02B2A58C6DB9384F9E062B348486C3C2BA33D388B7"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowShadeChannel.cs", "E313E42DCD0CEF8F940E1DEB358B292BE3DBAA09244190E82CA18140B53C1CA4"),
             new("csharp/src/SuperMetroid.Core/Game/MotherBrainDeathRomData.cs", "DCAA07FB5860D6DECD21FAC4D56B42DD386637C51572DE797B3D4BE780A69AA9"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainExplodedDoorPaintDefinitions.cs", "2ECEC273ABA4BC8147A1A9C73EA0C6718E95C9A739CC5325A23550136FB8C35B"),
             new("csharp/src/SuperMetroid.Core/Assets/MotherBrainFinalRoomPaintDefinitions.cs", "A821A07779A2C9C4065AE75DA484DBE17BEA8D560AE0FB18976177BD4AAB4815")]),
        new("SuperMetroid.Core.Assets.ChozoAndTubeColorCatalog", "chozo-tube-v1-complete-fixed-palettes",
            ["ApplyTubeCracks", "ApplyWreckedShip", "ApplyLowerNorfair"],
            [new("csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs", "708C3D693885FEBC5F375890B1D74E2379F95309F1AFD07BBBF3CB80957386F0"),
             new("csharp/src/SuperMetroid.Core/Game/ChozoAndTubeColorRomData.cs", "4BFF71FBD5A00111871F0743A814DF4D9324A3CB2F9DB54D6CBF7E3C2EC28BA5"),
             new("csharp/src/SuperMetroid.Core/Assets/ChozoStatuePaintDefinitions.cs", "FED550E229766018E20B588500E9FAF12F71A97C8F56DA8E8A37A0E85DE41954")]),
        new("SuperMetroid.Core.Assets.GameplayBasePaletteCatalog", "gameplay-base-v1-complete-cgram-image",
            ["LoadInitial", "LoadCommonSprites", "LoadEnemyProjectileSprites"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs", "D5236A08AB338E303E2C3041780F34AA49FE4AAB511DD1FC5B716BC818E12AB8"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesCgram.cs", "7484167AFD8A11931A9C036F734FFBA1CEF5EFA9D1B596177FF9C583CC5E4684"),
             new("csharp/src/SuperMetroid.Core/Hardware/SnesPpuLayout.cs", "2C0B763C5F05F8D3B37F4DBE93A0514973596F99EF0D07AAD7537C616B33A6F0")]),
        new("SuperMetroid.Core.Assets.SamusDeathPaletteArtworkCatalog", "samus-death-complete-cloned-color-sequences",
            ["SuitedColor", "SuitlessColor", "WhiteoutColor", "ExplosionPaletteIndex"],
            [new("csharp/src/SuperMetroid.Core/Assets/SamusDeathPaletteArtworkCatalog.cs", "25A85F0AE8DA898F4FEEA6462128946DDE0A0BDC01B1F0E4D4A9DDA8C91F8DC9"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6"),
             new("csharp/src/SuperMetroid.Core/Assets/SamusPaletteFade.cs", "84583825D0B2866162DEB582F4C4972B0BE3D3F2384D8E73A4762DB0A3FB08ED"),
             new("csharp/src/SuperMetroid.Core/Game/SamusPaletteRomData.cs", "063EA177518DE0CF5C8EA5BD064C2C20E19E1A9D796A632384DDC7F6113EAE2E"),
             new("csharp/src/SuperMetroid.Core/Game/SamusDeathExplosionTimingDefinitions.cs", "8C0C442727D4C01AE214E89B8238FAFCEBA7870AC61D1E9C13B07376AA4DF304")]),
    ];
}
