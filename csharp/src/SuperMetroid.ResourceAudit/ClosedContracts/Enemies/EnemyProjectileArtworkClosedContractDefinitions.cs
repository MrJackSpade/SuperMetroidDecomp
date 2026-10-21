namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Validated direct/program projectile compositions, including complete legacy inheritance.</summary>
internal static class EnemyProjectileArtworkClosedContractDefinitions
{
    /// <summary>Requires direct and instruction-selected enemy projectile frames, including inherited legacy frames, to resolve from installed artwork.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Enemies.Artwork.EnemyProjectileSpritemapCatalog", "installed-enemy-projectile-direct-and-program-frames",
            ["Get", "GetProgramFrame"],
            [new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyProjectileSpritemapCatalog.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Artwork/EnemyProjectileSpritemapDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Projectiles/EnemyProjectilePresentationFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Projectiles/EnemyProjectilePresentationFrameSets.cs"),
             new("csharp/src/SuperMetroid.Core/Bosses/MotherBrain/Artwork/MotherBrainEffectFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Projectiles/Artwork/MiscDustProjectileFrameDefinitions.cs"),
             new("csharp/src/SuperMetroid.Core/Enemies/Deaths/Artwork/EnemyDeathProjectileFrameDefinitions.cs")]),
    ];
}
