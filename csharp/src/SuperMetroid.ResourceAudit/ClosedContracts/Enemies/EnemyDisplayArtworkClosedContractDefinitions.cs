namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Actual display artwork availability, separate from non-resource ID projection.</summary>
internal static class EnemyDisplayArtworkClosedContractDefinitions
{
    /// <summary>
    /// Closed source sets for the import pipeline's installed ordinary spritemaps and every installed enemy frame's display
    /// gameplay draws by (#1463, #1513, #1603).
    /// </summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.AssetExtraction.Enemies.EnemySpritemapCatalog", "installed-simple-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemapCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemapSelection.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemaps.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoStride.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoStrideDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemapParts.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemySpritemapDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemySpritemapFrameList.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemySpritemapDocumentOrder.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyFrameNameDefinitions.cs")]),
        new("SuperMetroid.Core.Enemies.Artwork.EnemyFrameDisplays", "installed-enemy-frame-displays", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyFrameDisplays.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/InstalledEnemyFrameDisplays.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemapCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemapSelection.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemaps.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemySpritemapDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemySpritemapFrameList.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemySpritemapDocumentOrder.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyExtendedFrameCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyExtendedFrameSelection.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyExtendedFrames.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyExtendedDrawComponent.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/GoldenTorizoStrideDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemyExtendedFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemyExtendedFrameBounds.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemyExtendedFrameSequence.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Artwork/BossSpriteFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemyExtendedFrameDocumentOrder.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyFrameNameDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemapParts.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Drawing/CommonEnemyEmptyFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Drawing/CommonEnemyEmptyFrames.cs")]),
    ];
}
