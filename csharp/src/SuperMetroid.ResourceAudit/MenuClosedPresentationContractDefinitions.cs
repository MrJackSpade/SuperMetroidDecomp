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
                "B7B17351DF7ED1B519921F3B7E23C192F845A8161689554755B920EFEAD0BC75"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOptionsRomData.cs",
                "8F8F130E8D4BA5F2B82BC1CAD64C87DA2E5EA8611AD4BDD42368897EA769E3DA")],
            "The private-constructor loader requires all six pages, seven controller labels/anchors, " +
            "two special toggles, four language regions, three heading/cursor sets and four cursor frames. " +
            "Controller/cursor indices are guarded and language/background operations use fixed loaded fields. " +
            "Named page/menu/toggle selections additionally require independently resolved finite installed keys."),
        new("SuperMetroid.Core.Assets.GameOverPresentation", "game-over-v1-complete-enum-domains",
            ["LoadTilemapTo", "DrawBaby", "DrawEgg", "DrawCursor", "ApplyBabyPalette"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs",
                "4875F1E29748B33C81352529184423EAB9DC704E2CDC10D6F4CCAA7CCFC2C7E5"),
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
                "91B42EF6CBFC71449DE432F00259F0D0D2DB00DABB97E83286F49EBF3CC6F21C"),
             new("csharp/src/SuperMetroid.Core/Assets/PauseReserveUiDefinitions.cs",
                "C8423F78288613A457AA84868185C2A08650322A63A74A78B5AD4B12DFA57A88")],
            "The sole private-constructor loader requires Mode/ReserveTank/Manual/Auto labels, ten digit cells, " +
            "ten arrow cells and thirty-two complete color frames. Digit selectors are range-checked; " +
            "arrow animation masks to the complete 32-frame set and fixed colors/palettes are loaded. " +
            "Label selection additionally requires finite installed names, including both conditional branches."),
    ];
}
