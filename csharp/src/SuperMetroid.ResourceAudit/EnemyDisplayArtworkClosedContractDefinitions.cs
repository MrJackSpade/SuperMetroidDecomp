namespace SuperMetroid.ResourceAudit;

/// <summary>Actual display artwork availability, separate from non-resource ID projection.</summary>
internal static class EnemyDisplayArtworkClosedContractDefinitions
{
    /// <summary>Closed source sets for installed ordinary spritemaps and extended enemy display frames.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemySpritemapCatalog", "installed-simple-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/ChozoStrideGeometryDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/BabyMetroidSpriteParts.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/BabyMetroidCompositionDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/EnemySpritemapParts.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapDefinitions.cs")]),
        new("SuperMetroid.Core.Assets.EnemyExtendedFrameCatalog", "installed-extended-enemy-display-frames", ["TryGetDisplay"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/GoldenTorizoStrideGeometryDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameSequence.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/BossOamFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/PirateArtworkNameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/ChozoStrideGeometryDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/BabyMetroidSpriteParts.cs"),
             new("csharp/src/SuperMetroid.Core/Assets/BabyMetroidCompositionDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Hardware/EnemySpritemapParts.cs"),
             new("csharp/src/SuperMetroid.Core/Game/CommonEnemyEmptyExtendedFrameDefinitions.cs")]),
    ];
}
