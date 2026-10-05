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
                "81838DF879D36C9C812B06E2A851EBFA176EA1A38F37F1BFD58C43EBA248C6C4"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOptionsRomData.cs",
                "6402E8F44472D8FE1481FC111C546ED638FE28A11EB297F6A048318879CE262A")],
            "The private-constructor loader requires all six pages, seven controller labels/anchors, " +
            "two special toggles, four language regions, three heading/cursor sets and four cursor frames. " +
            "Controller/cursor indices are guarded. Stock geometry calculates from the reviewed native rules; independently edited geometry retains validated supplied fields. " +
            "Named page/menu/toggle selections additionally require independently resolved finite installed keys."),
        new("SuperMetroid.Core.Assets.GameOverPresentation", "game-over-v1-complete-enum-domains",
            ["LoadTilemapTo", "DrawBaby", "DrawEgg", "DrawCursor", "ApplyBabyPalette"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs",
                "0014DB87409BE7A7FB41DE3DF3E2FE7D38E924868FB49378A4BF78C9B3DDED05"),
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
                "A25DB1206665061D7B2E4E947449079E2ABC1499B6A2CD34688A8F1EF53B6C78"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs",
                "B435BE1CAD4C5440A35090E6F8785EF44B0795FBAF16543A8FCBE81216149D55")],
            "The sole private-constructor loader requires Mode/ReserveTank/Manual/Auto labels, ten digit cells, " +
            "ten arrow cells and thirty-two complete color frames. Digit selectors are range-checked; " +
            "arrow animation masks to the complete 32-frame set and fixed colors/palettes are loaded. " +
            "Label selection additionally requires finite installed names, including both conditional branches."),
    ];
}
