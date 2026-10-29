
namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Exact native aliases added for #1163; query false does not imply unowned artwork exists.</summary>
internal static class NativeArtworkAliasContractDefinitions
{
    /// <summary>Closed source sets for the escape timer's native DMA address aliases; the HUD's characters are the import's (#1466).</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Escape.Artwork.EscapeTimerTileAtlas", "native-EscapeTimerTileAtlas-dma-aliases", ["TryResolve"],
            [new("csharp/src/SuperMetroid.Core/Escape/Artwork/EscapeTimerTileAtlas.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Areas/Escape/EscapeTimerGlyphDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Areas/Escape/EscapeTimerGlyphs.cs")]),
    ];
}
