using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>How Phantoon's room lists his parts (#2252, #2556): his eye, tentacles and mouth with his body.</summary>
internal static partial class PhantoonPartListingDefinitions
{
    /// <summary>His body, then his eye, his tentacles and his mouth.</summary>
    internal static EnemyComposite<EnemyDefinitionId> Listing { get; } =
        new([EnemyDefinitionId.PhantoonBody, EnemyDefinitionId.PhantoonEye, EnemyDefinitionId.PhantoonTentacles, EnemyDefinitionId.PhantoonMouth]);
}
