namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Actual display artwork availability, separate from non-resource ID projection.</summary>
internal static class EnemyDisplayArtworkClosedContractDefinitions
{
    /// <summary>
    /// Closed source sets for the import pipeline's installed ordinary spritemaps and the extended enemy frames' displays
    /// gameplay draws (#1463, #1513).
    /// </summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.AssetExtraction.Enemies.EnemySpritemapCatalog", "installed-simple-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemapCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemapSelection.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemaps.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/ChozoStrideGeometryDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/ChozoStrideArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoStride.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Chozo/ChozoStrideDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/MotherBrain/BabyMetroidSpriteParts.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/MotherBrain/BabyMetroidCompositionDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemapParts.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemySpritemapDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemySpritemapFrameList.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemySpritemapCompositions.cs")]),
        new("SuperMetroid.Core.Enemies.Artwork.EnemyExtendedFrameDisplayCatalog", "installed-extended-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyExtendedFrameDisplayCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyExtendedFrameDisplaySelection.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyExtendedFrameDisplays.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyExtendedFrameCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyExtendedFrameSelection.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyExtendedFrames.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyExtendedDrawComponent.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Torizo/GoldenTorizoStrideGeometryDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Torizo/GoldenTorizoStrideArtwork.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/GoldenTorizoStrideDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemyExtendedFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemyExtendedFrameBounds.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/World/Artwork/EnemyExtendedFrameSequence.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Bosses/Artwork/BossSpriteFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyExtendedFrameCompositions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemySpritemapParts.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Drawing/CommonEnemyEmptyFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Drawing/CommonEnemyEmptyFrames.cs")]),
    ];
}
