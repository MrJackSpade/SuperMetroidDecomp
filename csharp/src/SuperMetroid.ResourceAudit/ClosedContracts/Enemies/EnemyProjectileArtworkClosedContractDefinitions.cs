namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Validated direct/program projectile compositions, including complete legacy inheritance.</summary>
internal static class EnemyProjectileArtworkClosedContractDefinitions
{
    /// <summary>Requires direct and instruction-selected enemy projectile frames, including inherited legacy frames, to resolve from installed artwork.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.AssetExtraction.Enemies.EnemyProjectileSpritemapCatalog", "installed-enemy-projectile-direct-and-program-frames",
            ["Get", "GetProgramFrame"],
            [new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyProjectileSpritemapCatalog.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyProjectileSpritemapSelection.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyProjectileSpritemaps.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyProjectileSpritemapDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyProjectileFrames.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyProjectilePresentationFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.AssetExtraction/Enemies/EnemyProjectilePresentationFrameSets.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainEffectFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Projectiles/Artwork/MiscDustProjectileFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Deaths/Artwork/EnemyDeathProjectileFrameDefinitions.cs")]),
    ];
}
