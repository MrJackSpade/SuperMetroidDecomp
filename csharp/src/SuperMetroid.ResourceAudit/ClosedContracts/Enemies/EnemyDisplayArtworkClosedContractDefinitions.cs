namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Actual display artwork availability, separate from non-resource ID projection.</summary>
internal static class EnemyDisplayArtworkClosedContractDefinitions
{
    /// <summary>Closed source sets for installed ordinary spritemaps and extended enemy display frames.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Enemies.Artwork.EnemySpritemapCatalog", "installed-simple-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemySpritemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/ChozoStrideGeometryDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/BabyMetroidSpriteParts.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/BabyMetroidCompositionDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/EnemySpritemapParts.cs"),
             new("csharp/src/SuperMetroid.Core/World/Artwork/EnemySpritemapDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemySpritemapCompositions.cs")]),
        new("SuperMetroid.Core.Enemies.Artwork.EnemyExtendedFrameCatalog", "installed-extended-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyExtendedFrameCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Torizo/Artwork/GoldenTorizoStrideGeometryDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/World/Artwork/EnemyExtendedFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/World/Artwork/EnemyExtendedFrameSequence.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/Artwork/BossOamFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyExtendedFrameCompositions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemySpritemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/ChozoStrideGeometryDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/BabyMetroidSpriteParts.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/BabyMetroidCompositionDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/EnemySpritemapParts.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Drawing/CommonEnemyEmptyExtendedFrameDefinitions.cs")]),
    ];
}
