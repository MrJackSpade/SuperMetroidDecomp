namespace SuperMetroid.ResourceAudit;

/// <summary>Reviewed text and map loaders with complete finite resource domains, not cinematic or navigation tests.</summary>
internal static class TextAndMapClosedContractDefinitions
{
    /// <summary>Stores all.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EscapeTypewriterPresentation", "escape-text-complete-program-membership", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.IntroNarrationPresentation", "narration-complete-compiled-pages", ["GetLines", "Compile"],
            [new("csharp/src/SuperMetroid.Core/Assets/IntroNarrationPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/IntroNarrationDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.EndingTextPresentation", "ending-text-complete-panels-and-sequences", ["Compile", "BuildResultPanel", "BuildCopyrightPanel"],
            [new("csharp/src/SuperMetroid.Core/Assets/EndingTextLayoutDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EndingTextPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EndingTextDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.CreditsPresentation", "credits-loaded-complete-row-domain", ["GetRow"],
            [new("csharp/src/SuperMetroid.Core/Assets/CreditsPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/CreditsPresentationDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EndingTextDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.MapSpriteCatalog", "map-sprite-complete-named-compositions", ["Draw", "LoadArtworkTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapSpriteCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MapMarkerGeometry.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/WorldMapLabelComposition.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuSmallFontArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuLargeFontArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuCompactLetteringArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuPanelTileArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuShoulderButtonArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuShoulderHighlightArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MapMarkerTileArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuBeveledSquareArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MenuThinBorderArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MapObjectTileArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapSpriteDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.MapArrowPresentation", "map-arrow-complete-direction-domain", ["Get"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapArrowPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/MenuSelectorTiming.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapArrowDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/MapScrollControls.cs"),
             new("csharp/src/SuperMetroid.Core/Frontend/FileSelectMapScroll.cs")]),
        new("SuperMetroid.Core.Assets.MapScreenPresentation", "map-screen-complete-pages-and-bounded-names", ["LoadTo"],
            [new("csharp/src/SuperMetroid.Core/Assets/MapScreenPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/MapScreenDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Game/AreaId.cs")]),
    ];
}
