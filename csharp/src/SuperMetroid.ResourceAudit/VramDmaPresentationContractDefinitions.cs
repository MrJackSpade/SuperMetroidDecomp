namespace SuperMetroid.ResourceAudit;

/// <summary>Exact native aliases added for #1163; query false does not imply unowned artwork exists.</summary>
internal static class VramDmaPresentationContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.HudTileAtlas", "native-HudTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.EscapeTimerTileAtlas", "native-EscapeTimerTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EscapeTimerGlyphDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.GrappleTileAtlas", "native-GrappleTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleBeamTilePatterns.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/SnesPlanarTileEncoder.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs")]),
    ];
}
