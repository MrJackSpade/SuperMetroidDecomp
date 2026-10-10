namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed complete menu providers; named arguments still need independent key coverage.</summary>
internal static class MenuClosedPresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.GameOptionsPresentation", "options-v1-complete-menu-domains",
            ["CreatePage", "LoadBackground", "ApplyLanguage", "ApplyControllerLabel", "ApplySpecialToggle",
                "CursorPosition", "DrawHeading", "DrawCursor"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs", "CC436FDF0837F5BE2E3FEE426C882F3C828D7B724DF348BF06C6E7A5B3C4002E"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "3618A0B1A42C5AA6AC26605CAF4FCF2AE0438E98DF4B6C6BDEC2AAA377477F0F"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "AEDCBBB85A3FEDC7E67BD630B2CF046E353EC83DBB40E2D7B2A8AB66D7225A90"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "54DF046050D52709780E4E0A4645625D4647EF41F72C3DEA175FA34D0FA3B0F6"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs", "066962DCF7802C8FC565BD19FC4C0865C1693652A874F5F6894B1D8B9CAB36BF"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs", "C32C723DBBABA4FDEF82FCFFE9AC194861391652F68C05581DC80824F5275032"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOptionsRomData.cs", "95B7DF75F36B10FD445714E9F417496EFB72FC1147481A67A3744D87452EAD7B")]),
        new("SuperMetroid.Core.Assets.GameOverPresentation", "game-over-v1-complete-enum-domains",
            ["LoadTilemapTo", "DrawBaby", "DrawEgg", "DrawCursor", "ApplyBabyPalette"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs", "C176B0ED4931986265ADBBCB39DD4CCAD1798D678384F1A1F1734DF08D0445C8"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "3618A0B1A42C5AA6AC26605CAF4FCF2AE0438E98DF4B6C6BDEC2AAA377477F0F"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "AEDCBBB85A3FEDC7E67BD630B2CF046E353EC83DBB40E2D7B2A8AB66D7225A90"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "54DF046050D52709780E4E0A4645625D4647EF41F72C3DEA175FA34D0FA3B0F6"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverSpriteParts.cs", "99CB835A574D11E8021EFC5834C28516764CE7BB0883D1A22B78D1F2BC9ECEB1"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverBabyColorCatalog.cs", "CBE63F7471884D79E2916F455E28386B742EA62E2CDA9B91B46259A6AF20D207"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs", "398E98A457128DB634036693BD38C41E1D670D5B0357ADE3D37012F3346343E6"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverRomData.cs", "AC22FA7AABAF8C5AABBE2244C96F178F52F54EB043B4A68434B68D67BBD58B4F"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverBabyAnimationDefinitions.cs", "491C23C2C635E9841A90A4257C84B573723AF4808BB788C84ED133668B076F26")]),
        new("SuperMetroid.Core.Assets.PauseReserveUiPresentation", "pause-reserve-v1-complete-fields",
            ["ApplyLabel", "ApplyDigit", "ApplyArrowTilePalettes", "ApplyArrowColors"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs", "25FFFCA1F0587BF298EC1A2EB29DD3A3F64C6B07F6F26BBD1084AACA8653CA9B"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs", "677659877C825CD0CB7CE19BF9F627A3723859123E43F71A5188F4D3DFC1C301")]),
    ];
}
