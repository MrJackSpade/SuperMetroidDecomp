using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed source-owned interface dispatch sets; external injected implementations are not certified.</summary>
internal static class InterfacePresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.IPaletteFxColorSource", "palette-interface-total-membership-query", ["TryReadColor"],
            [Source("IPaletteFxColorSource", "333744D4FCB6A10AA545F9483B83E8743512834D810626BF24DC4497BDF48070"),
             Source("RoomPaletteFxPresentation", "F9BC761D7CE8F979144571D7CAE6E984A57EE0836A91F80E3AB0028707C73341"),
             Source("LoadingPaletteInputView", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6"),
             Source("LoadingPaletteColorDefinitions", "DCB60356933F8D5A3C97E5F22DFADEAB5551CF43361986122D252D60FBF0A25C"),
             Source("HeatPaletteColorDefinitions", "CC6A9020C7166ED6E7DE86BBD107B4501A13DE8A96A83246D8DC56FE7FFF358D"),
             new("csharp/src/SuperMetroid.Core/Game/PaletteFxHeatInstructionListDefinitions.cs", "6C7D12A36E5C66CB6B3C7E170FEB9CFF79C4D83446B1B5B2C305C2014CA08231"),
             Source("HeatPaletteInputView", "7EEE5F6A5FA69DDA9E7BE1A656E32C28EE5CB0F8A9609347D43BBEFF0DB602F3"),
             Source("TitlePalettePresentation", "A1FAA8D0D21E5D3C5320FE2F0C023EFF8F39CD80258087E0C5D4499FB215D904")]),
        new("SuperMetroid.Core.Assets.IIntroCinematicSpritePresentation", "cinematic-interface-complete-owned-frame-domains", ["Draw"],
            [Source("IIntroCinematicSpritePresentation", "8EFBE3B6148D264AFF2F317970B1E3DCD8A372F46E8C142AA1C4527645B699E6"),
             Source("IntroDiscoveryActorSpritePresentation", "A71A19E518FC3CB8D6F8955652F342DBCE9035FB9AD5FCDCE8A232C6E8571E39"),
             Source("IntroDiscoveryActorSpriteDefinitions", "F4B9C5FC3DC3885CE6C8CC9770E6507606514BEB31FEDA1EFCC241E7A11E101D"),
             Source("IntroEggEffectSpritePresentation", "C7ABCAA4C70D980F57AE0A43994B062346D5652CE11DB41EFBFBBEAB4D81F684"),
             Source("IntroEggEffectSpriteDefinitions", "F21C5CB4D995F4F1F12082E021ECBEDC4DDB17E7A336E1EBAA9B12098D59E9E1"),
             Source("IntroRinkaSpritePresentation", "80714E231675E0AF52FD769A56F09EA38A40FD2BCEFAAA831F2EA40ABF60CBD2"),
             Source("IntroRinkaSpriteDefinitions", "E1B962B0AD185ACE3C864B48F26CD4D03CE87058041E2C4041C6589FDC3A9FC1"),
             Source("IntroScientistSpritePresentation", "B83EF77F4F828989174FE990E002911EB9C1FF976858511EF84C479F0744A20D"),
             Source("IntroScientistSpriteDefinitions", "AC9E9C68F63AF4B98853C746CEB3C5703570172357CA79A6572AA3CB067959BD"),
             Source("CeresFlightSpritePresentation", "0DF3C0C24C053168FF561DBD90BD7A96E431D6C9F17341618FA1A8348A7E4035"),
             Source("CeresDestructionSpritePresentation", "1A2D26370B2D2A3785FE312E4C348A757D3E456CBEEC748CAF38B6C4FD3437E6"),
             Source("EndingCloudSpritePresentation", "03E640AF4B4C23DC3384B24761DC2CFD8EAB685CB5D8501E7BF8B85B05444FE8"),
             Source("EndingCompletionTextSpritePresentation", "D7C411410FBFF5534FE6262B9A3A03DF54BD1246C064442B956D0B4D8B74493C"),
             Source("EndingExplosionSpritePresentation", "93BD7B3FB85897C2848F29196BFE7466CA48E3B8AC8112037F234A0665DB67F7"),
             Source("EndingLogoSpritePresentation", "2EC92F3D3EEFDC5926577F474D209642283F1FABCDD41502DE9DA3E02288BBE9"),
             Source("EndingRewardSpritePresentation", "35FE6676D2A62A31E1A8A20ECD05E2AA047E31C82ECB9235E14CFF17F0E5EE0A"),
             Source("IntroCinematicSpriteCompiler", "B7425C9529BBF7DE5EC6E220C69E70A36EF2FE6203F51E8179BFCB02CAFBA77F")]),
    ];

    private static ReviewedSource Source(string name, string hash) => new("csharp/src/SuperMetroid.Core/Assets/" + name + ".cs", hash);

    internal static string[]? Implementations(string type) => type switch
    {
        "SuperMetroid.Core.Assets.IPaletteFxColorSource" => [typeof(RoomPaletteFxPresentation).FullName!, typeof(TitlePalettePresentation).FullName!],
        "SuperMetroid.Core.Assets.IIntroCinematicSpritePresentation" =>
            [typeof(IntroDiscoveryActorSpritePresentation).FullName!, typeof(IntroEggEffectSpritePresentation).FullName!,
             typeof(IntroRinkaSpritePresentation).FullName!, typeof(IntroScientistSpritePresentation).FullName!,
             typeof(CeresFlightSpritePresentation).FullName!, typeof(CeresDestructionSpritePresentation).FullName!,
             typeof(CeresSceneSpritePresentation).FullName!, typeof(EndingCloudSpritePresentation).FullName!,
             typeof(EndingCompletionTextSpritePresentation).FullName!, typeof(EndingExplosionSpritePresentation).FullName!,
             typeof(EndingLogoSpritePresentation).FullName!, typeof(EndingRewardSpritePresentation).FullName!],
        _ => null,
    };
}
