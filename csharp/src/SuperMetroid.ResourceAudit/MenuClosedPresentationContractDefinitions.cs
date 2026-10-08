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
                "21AFA013DD4FABF1547C633977AB35998936233FE97F600F5E8FB809D77C4A67"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "155C4933434355E65FA4AFD066701B8720222BB5F0B38020FC170048BBDC0DEE"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "92CFE18B185C1526FACE125CF345944D1D878525935BE219DC21EE19838E3E9E"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "718108CB3359B183DF55EBB1F4C86D41E0858FC5DFE1AF6A01FC0612E69AD0FF"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs", "A492AC3C7692DA38E0E838CAB60DC605DDC8E875DBE098475EBAC530C0E47372"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs", "6B79556FD47098253A8A977E398C98A1908D01A7DD4E9E5916A38634A33E0080"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOptionsRomData.cs",
                "6402E8F44472D8FE1481FC111C546ED638FE28A11EB297F6A048318879CE262A")]),
        new("SuperMetroid.Core.Assets.GameOverPresentation", "game-over-v1-complete-enum-domains",
            ["LoadTilemapTo", "DrawBaby", "DrawEgg", "DrawCursor", "ApplyBabyPalette"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs",
                "2FC5935194BD3185454457C6F1AA4AA6F4AD81227630BD7B3E01F08F4C6419CC"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "155C4933434355E65FA4AFD066701B8720222BB5F0B38020FC170048BBDC0DEE"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "92CFE18B185C1526FACE125CF345944D1D878525935BE219DC21EE19838E3E9E"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "718108CB3359B183DF55EBB1F4C86D41E0858FC5DFE1AF6A01FC0612E69AD0FF"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverSpriteParts.cs", "A8A9E2DADD7BED8A1BC57C9C3C6FF072CA4B285BB51267C5EA07D7853FF76461"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverBabyColorCatalog.cs",
                "EC18D16A0969A3B6E8FE2ED0907FBF84B4A791E95D8BD3EA2EA614E8DAA83141"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs",
                "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverRomData.cs",
                "D73477EFE4E00EF13D43E2C7DDCB6E9232B84A2CA736592168CC194E59017C44"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverBabyAnimationDefinitions.cs",
                "5EE3A896D0791B4BBC0C7D06A0EE1B1467CFCC519FAB230C8862842443B8D504")]),
        new("SuperMetroid.Core.Assets.PauseReserveUiPresentation", "pause-reserve-v1-complete-fields",
            ["ApplyLabel", "ApplyDigit", "ApplyArrowTilePalettes", "ApplyArrowColors"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs",
                "123A68E03DBCE63AA5A6F43C6FE5BDAC3A2AED0113E2E5B5FE9E2CA895493C16"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs",
                "B435BE1CAD4C5440A35090E6F8785EF44B0795FBAF16543A8FCBE81216149D55")]),
    ];
}
