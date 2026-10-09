namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed complete menu providers; named arguments still need independent key coverage.</summary>
internal static class MenuClosedPresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.GameOptionsPresentation", "options-v1-complete-menu-domains",
            ["CreatePage", "LoadBackground", "ApplyLanguage", "ApplyControllerLabel", "ApplySpecialToggle",
                "CursorPosition", "DrawHeading", "DrawCursor"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs",
                "CC436FDF0837F5BE2E3FEE426C882F3C828D7B724DF348BF06C6E7A5B3C4002E"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "3618A0B1A42C5AA6AC26605CAF4FCF2AE0438E98DF4B6C6BDEC2AAA377477F0F"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "AEDCBBB85A3FEDC7E67BD630B2CF046E353EC83DBB40E2D7B2A8AB66D7225A90"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "54DF046050D52709780E4E0A4645625D4647EF41F72C3DEA175FA34D0FA3B0F6"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs", "3C500B0BC39445C8C6EECAE9DD9D34592239BDE1D5255DA05665434C969645DA"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs", "C32C723DBBABA4FDEF82FCFFE9AC194861391652F68C05581DC80824F5275032"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOptionsRomData.cs",
                "95B7DF75F36B10FD445714E9F417496EFB72FC1147481A67A3744D87452EAD7B")]),
        new("SuperMetroid.Core.Assets.GameOverPresentation", "game-over-v1-complete-enum-domains",
            ["LoadTilemapTo", "DrawBaby", "DrawEgg", "DrawCursor", "ApplyBabyPalette"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs",
                "37D8107A92757ECDCE6EE39A41FF113F5967A53137E7E816407C4C8BB00F561C"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "3618A0B1A42C5AA6AC26605CAF4FCF2AE0438E98DF4B6C6BDEC2AAA377477F0F"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "AEDCBBB85A3FEDC7E67BD630B2CF046E353EC83DBB40E2D7B2A8AB66D7225A90"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "54DF046050D52709780E4E0A4645625D4647EF41F72C3DEA175FA34D0FA3B0F6"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverSpriteParts.cs", "99CB835A574D11E8021EFC5834C28516764CE7BB0883D1A22B78D1F2BC9ECEB1"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverBabyColorCatalog.cs",
                "90646187596F06B46B9727BFCDF10A6159CC4CC92B3DC578D419524A55E0DD64"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs",
                "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverRomData.cs",
                "AC22FA7AABAF8C5AABBE2244C96F178F52F54EB043B4A68434B68D67BBD58B4F"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverBabyAnimationDefinitions.cs",
                "491C23C2C635E9841A90A4257C84B573723AF4808BB788C84ED133668B076F26")]),
        new("SuperMetroid.Core.Assets.PauseReserveUiPresentation", "pause-reserve-v1-complete-fields",
            ["ApplyLabel", "ApplyDigit", "ApplyArrowTilePalettes", "ApplyArrowColors"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs",
                "99FB9EA5533531AB36B53FA6F71E2216EC20D034B29F397135AFDDD3A257C56F"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs",
                "677659877C825CD0CB7CE19BF9F627A3723859123E43F71A5188F4D3DFC1C301")]),
    ];
}
