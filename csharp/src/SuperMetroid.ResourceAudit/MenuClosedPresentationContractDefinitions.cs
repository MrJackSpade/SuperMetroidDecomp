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
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "6FDADA7BC65E29CCCA60F6D9301DD5B6020202553A025A2994AADA8BE497DA28"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "92CFE18B185C1526FACE125CF345944D1D878525935BE219DC21EE19838E3E9E"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "718108CB3359B183DF55EBB1F4C86D41E0858FC5DFE1AF6A01FC0612E69AD0FF"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuHeadingBorderDefinitions.cs", "40ECFBF2ADE308FC71BB90B1EEC1A00E70308F9D49DB9F190C9614E2621380E2"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBorderParts.cs", "6B79556FD47098253A8A977E398C98A1908D01A7DD4E9E5916A38634A33E0080"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOptionsRomData.cs",
                "6402E8F44472D8FE1481FC111C546ED638FE28A11EB297F6A048318879CE262A")],
            "The private-constructor loader requires all six pages, seven controller labels/anchors, " +
            "two special toggles, four language regions, three heading/cursor sets and four cursor frames. " +
            "Controller/cursor indices are guarded. Stock geometry calculates from the reviewed native rules; independently edited geometry retains validated supplied fields. " +
            "Named page/menu/toggle selections additionally require independently resolved finite installed keys."),
        new("SuperMetroid.Core.Assets.GameOverPresentation", "game-over-v1-complete-enum-domains",
            ["LoadTilemapTo", "DrawBaby", "DrawEgg", "DrawCursor", "ApplyBabyPalette"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs",
                "2FC5935194BD3185454457C6F1AA4AA6F4AD81227630BD7B3E01F08F4C6419CC"),
             new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs", "6FDADA7BC65E29CCCA60F6D9301DD5B6020202553A025A2994AADA8BE497DA28"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuMissileAnimationDefinitions.cs", "92CFE18B185C1526FACE125CF345944D1D878525935BE219DC21EE19838E3E9E"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCursorParts.cs", "718108CB3359B183DF55EBB1F4C86D41E0858FC5DFE1AF6A01FC0612E69AD0FF"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverSpriteParts.cs", "A8A9E2DADD7BED8A1BC57C9C3C6FF072CA4B285BB51267C5EA07D7853FF76461"),
             new("csharp/src/SuperMetroid.Core/Assets/GameOverBabyColorCatalog.cs",
                "EC18D16A0969A3B6E8FE2ED0907FBF84B4A791E95D8BD3EA2EA614E8DAA83141"),
             new("csharp/src/SuperMetroid.Core/Assets/LoadingPaletteInputView.cs",
                "75AD4BE101D4CF0C9230325B23C505015933CFAF99B6D40135050635A5AB4CD9"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverRomData.cs",
                "93546CA50CA9AB570A2F09AF939163AFB5B10420A4DD360BAED521A9FD46104A"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverBabyAnimationDefinitions.cs",
                "A8617F78639973FB3E9BDD260CC8CE7B38C1259248CF512C86F7039AB7FD3360")],
            "Private construction requires the full tilemap, all eight named sprites and all four baby palettes. " +
            "Baby frame/palette enum switches are exhaustive over their supported domains and reject invalid values; " +
            "all four cursor frames are required and bounds-checked. Egg/tilemap operations use fixed loaded resources."),
        new("SuperMetroid.Core.Assets.PauseReserveUiPresentation", "pause-reserve-v1-complete-fields",
            ["ApplyLabel", "ApplyDigit", "ApplyArrowTilePalettes", "ApplyArrowColors"],
            [new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs",
                "123A68E03DBCE63AA5A6F43C6FE5BDAC3A2AED0113E2E5B5FE9E2CA895493C16"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs",
                "B435BE1CAD4C5440A35090E6F8785EF44B0795FBAF16543A8FCBE81216149D55")],
            "The sole private-constructor loader requires Mode/ReserveTank/Manual/Auto labels, ten digit cells, " +
            "ten arrow cells and thirty-two complete color frames. Digit selectors are range-checked; " +
            "arrow animation masks to the complete 32-frame set and fixed colors/palettes are loaded. " +
            "Label selection additionally requires finite installed names, including both conditional branches."),
    ];
}
