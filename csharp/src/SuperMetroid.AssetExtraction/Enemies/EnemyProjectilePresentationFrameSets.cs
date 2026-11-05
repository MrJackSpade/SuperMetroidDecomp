namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The installed program frames: the first program-frame catalog's frames and every later catalog's additions, in publishing order.</summary>
internal static class EnemyProjectilePresentationFrameSets
{
    /// <summary>Each catalog's additions, in publishing order.</summary>
    private static readonly IReadOnlyList<EnemyProjectilePresentationFrameDefinition>[] Additions =
    [
        EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizoAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreGoldenTorizoEggAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreTorizoEffectsAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreGenericEnemyDeathAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreEnvironmentAndAttackAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreMotherBrainAndStatueAdditions,
        EnemyProjectilePresentationFrameDefinitions.PreWorkRobotAdditions,
        EnemyProjectilePresentationFrameDefinitions.PrePolypRockAdditions,
        EnemyProjectilePresentationFrameDefinitions.CurrentAdditions,
    ];

    /// <summary>The program-frame set the installed artwork loader and its import read.</summary>
    private static readonly EnemyProjectilePresentationFrameDefinition[] Frames =
        [.. EnemyProjectilePresentationFrameDefinitions.FirstProgramFrames.Concat(Additions.SelectMany(added => added))];

    /// <summary>Every projectile program frame.</summary>
    internal static ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> All => Frames;
}
