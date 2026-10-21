using SuperMetroid.ResourceAudit.ClosedContracts;

namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>Exact native aliases added for #1163; query false does not imply unowned artwork exists.</summary>
internal static class VramDmaPresentationContractDefinitions
{
    /// <summary>Closed source sets for the HUD, escape timer, and grapple native DMA address aliases.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Hud.Artwork.HudTileAtlas", "native-HudTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Hud/Artwork/HudTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/GrappleTileDefinitions.cs")]),
        new("SuperMetroid.Core.Escape.Artwork.EscapeTimerTileAtlas", "native-EscapeTimerTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerGlyphDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/GrappleTileDefinitions.cs")]),
        new("SuperMetroid.Core.Samus.Artwork.GrappleTileAtlas", "native-GrappleTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Samus/Artwork/GrappleTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/GrappleBeamTilePatterns.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs"),
             new("csharp/src/SuperMetroid.Core/Samus/Artwork/GrappleTileDefinitions.cs")]),
    ];
}
