namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>Reviewed text and map loaders with complete finite resource domains, not cinematic or navigation tests.</summary>
internal static class TextAndMapClosedContractDefinitions
{
    /// <summary>Reviewed finite text, credits, and map presentation loaders with their guarded methods and source ownership files.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Escape.Artwork.EscapeTypewriterPresentation", "escape-text-complete-program-membership", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTypewriterPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTypewriterDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTypewriterLines.cs")]),
        new("SuperMetroid.Core.Frontend.Intro.Artwork.IntroNarrationPresentation", "narration-complete-compiled-pages", ["GetLines", "Compile"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroNarrationPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroNarrationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Intro/Artwork/IntroNarrationGlyphs.cs")]),
        new("SuperMetroid.Core.Frontend.Ending.Artwork.EndingTextPresentation", "ending-text-complete-panels-and-sequences", ["Compile", "BuildResultPanel", "BuildCopyrightPanel"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingTextLayoutDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingTextPanels.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingTextPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingTextDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingTextGlyphs.cs")]),
        new("SuperMetroid.Core.Frontend.Ending.Artwork.CreditsPresentation", "credits-loaded-complete-row-domain", ["GetRow"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/CreditsPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/CreditsPresentationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/CreditsGlyphs.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/CreditsRoles.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingTextDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Ending/Artwork/EndingTextGlyphs.cs")]),
        new("SuperMetroid.Core.Frontend.Map.Artwork.MapSpriteSheet", "map-sprite-sheet-complete-named-frames", ["Draw"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/MapSpriteSheet.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Map/MapSpriteDefinitions.cs")]),
        new("SuperMetroid.Core.Frontend.Map.Artwork.MapArrowPresentation", "map-arrow-complete-direction-domain", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/MapArrowPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Menus/MenuSelectorTiming.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Map/MapArrowDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Map/MapScrollControls.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/FileSelect/FileSelectMapScroll.cs")]),
        new("SuperMetroid.Core.Frontend.Map.Artwork.MapScreenPresentation", "map-screen-complete-pages-and-bounded-names", ["LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/MapScreenPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/MapScreenDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Progression/AreaId.cs")]),
    ];
}
