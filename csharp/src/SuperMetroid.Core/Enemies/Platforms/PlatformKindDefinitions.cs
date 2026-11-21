namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>The kind of platform each platform definition places (<see cref="PlatformsBehavior"/>).</summary>
internal static class PlatformKindDefinitions
{
    /// <summary>Each platform definition's kind, which its art shows: the Suspensor platform's, and the Tripper's.</summary>
    internal static readonly IReadOnlyDictionary<EnemyDefinitionId, PlatformKind> Kinds = new Dictionary<EnemyDefinitionId, PlatformKind>
    {
        [EnemyDefinitionId.SuspensorPlatform] = PlatformKind.SuspensorPlatform,
        [EnemyDefinitionId.Tripper] = PlatformKind.Tripper,
    };
}
