namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>The atomic seven-area install and its deliberately small VRAM source domain.</summary>
internal static class AreaMapClosedContractDefinitions
{
    /// <summary>Requires every selected area map to come from the atomic seven-area install and its reviewed tile-upload owners.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Frontend.Map.Artwork.AreaMapPresentation", "map-atomic-seven-areas-and-owned-uploads", ["Get", "Resolve"],
            [new("csharp/src/SuperMetroid.Core/Frontend/Map/Artwork/AreaMapPresentation.cs"),
             new("csharp/src/SuperMetroid.Core/Progression/AreaId.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/AreaMapPresentationAsset.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/AreaMapStockRules.cs"),
             new("csharp/src/SuperMetroid.Core/Hud/Artwork/HudTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Map/Artwork/MapTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerGlyphDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerGlyphs.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Kraid/KraidHudRestoration.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/IInstalledArtworkContent.cs")]),
    ];
}
