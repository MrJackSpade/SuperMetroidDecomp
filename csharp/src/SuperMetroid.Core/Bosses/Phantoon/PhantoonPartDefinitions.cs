using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The definitions the Phantoon species installs and the part each one is.</summary>
internal static class PhantoonPartDefinitions
{
    /// <summary>Each Phantoon definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, PhantoonPart> Parts { get; } = new Dictionary<EnemyDefinitionId, PhantoonPart>
    {
        [EnemyDefinitionId.PhantoonBody] = PhantoonPart.Body,
        [EnemyDefinitionId.PhantoonEye] = PhantoonPart.Eye,
        [EnemyDefinitionId.PhantoonMouth] = PhantoonPart.Mouth,
        [EnemyDefinitionId.PhantoonTentacles] = PhantoonPart.Tentacles,
    };
}
