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
                "0B30B28A4FA3E22A1AF2D90A62BD673C559AA640DACD22181FA185D16BECDB6B")],
            "The private-constructor loader requires all six pages, seven controller labels/anchors, " +
            "two special toggles, four language regions, three heading/cursor sets and four cursor frames. " +
            "Controller/cursor indices are guarded and language/background operations use fixed loaded fields. " +
            "Named page/menu/toggle selections additionally require independently resolved finite installed keys."),
        new("SuperMetroid.Core.Assets.GameOverPresentation", "game-over-v1-complete-enum-domains",
            ["LoadTilemapTo", "DrawBaby", "DrawEgg", "DrawCursor", "ApplyBabyPalette"],
            [new("csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs",
                "A07C619692FB74869E6781A35DAB12DF9F595D5575530CFFE9941A099A39FB81"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverRomData.cs",
                "D4898ADBE50B570C97A08BED0BFD06D5AF53009406849FE0E307F8A68B649891"),
             new("csharp/src/SuperMetroid.Core/Frontend/GameOverBabyAnimationDefinitions.cs",
                "8DEAD1215A895068FF927725551EF880A3BC7D2DA1C50271662BC43F5384EE5F")],
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
