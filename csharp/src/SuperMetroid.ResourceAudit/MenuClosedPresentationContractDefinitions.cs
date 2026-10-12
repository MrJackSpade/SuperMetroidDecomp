namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed complete menu providers; named arguments still need independent key coverage.</summary>
internal static class MenuClosedPresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.GameOptionsPresentation", "options-v1-complete-menu-domains",
            ["CreatePage", "LoadBackground", "ApplyLanguage", "ApplyControllerLabel", "ApplySpecialToggle",
                "CursorPosition", "DrawHeading", "DrawCursor"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs", "4A04BD1AFEF19B43622855EBFC4E0B9F8D5AD1E5CD0587412B62281B4C8FEF4E"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "72753125C34DD2B35620C322E0F6406386172EE65A50E0D4A2878D761E7B5CB8"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "AEDCBBB85A3FEDC7E67BD630B2CF046E353EC83DBB40E2D7B2A8AB66D7225A90"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "54DF046050D52709780E4E0A4645625D4647EF41F72C3DEA175FA34D0FA3B0F6"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs", "066962DCF7802C8FC565BD19FC4C0865C1693652A874F5F6894B1D8B9CAB36BF"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs", "C32C723DBBABA4FDEF82FCFFE9AC194861391652F68C05581DC80824F5275032"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOptionsRomData.cs", "6C299B810017AA144473D9E5C9299B528079F50B6F72F9FE7942C929AF042F9B")]),
        new("SuperMetroid.Core.Assets.GameOverPresentation", "game-over-v1-complete-enum-domains",
            ["LoadTilemapTo", "DrawBaby", "DrawEgg", "DrawCursor", "ApplyBabyPalette"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs", "0279B1AB23CE7A3C713338E58C1C50489138941DB0E8585B5D7319139E1A05E3"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "72753125C34DD2B35620C322E0F6406386172EE65A50E0D4A2878D761E7B5CB8"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "AEDCBBB85A3FEDC7E67BD630B2CF046E353EC83DBB40E2D7B2A8AB66D7225A90"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "54DF046050D52709780E4E0A4645625D4647EF41F72C3DEA175FA34D0FA3B0F6"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverSpriteParts.cs", "99CB835A574D11E8021EFC5834C28516764CE7BB0883D1A22B78D1F2BC9ECEB1"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverBabyColorCatalog.cs", "CE42C973EE1B7CAA438BE216CCEE2A8BDF6AA6BF79A286027F20375279AE489A"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverRomData.cs", "839FC372BBF82F2717C005AF05B03D6131B4479138012C3379A2422C022DA598"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverBabyAnimationDefinitions.cs", "FAF8E88BD26419FC189CD8C9D08D8AAB50CA7A9A2E2A1A113AABF342D3433C0A")]),
        new("SuperMetroid.Core.Assets.PauseReserveUiPresentation", "pause-reserve-v1-complete-fields",
            ["ApplyLabel", "ApplyDigit", "ApplyArrowTilePalettes", "ApplyArrowColors"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs", "352E631C5341277CCCFF7CB7AD3F46DB0107FCE0BBBB020AF6534D06B69A51F9"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs", "677659877C825CD0CB7CE19BF9F627A3723859123E43F71A5188F4D3DFC1C301")]),
    ];
}
