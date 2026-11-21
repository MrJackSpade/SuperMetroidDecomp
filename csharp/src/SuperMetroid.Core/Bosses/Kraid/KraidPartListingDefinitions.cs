using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// How Kraid's room lists his parts (#2556): his arm, his top, middle and bottom lint, his foot and his two fingernails
/// right after his body, the fixed places after his body's at which the game reaches them.
/// </summary>
internal static class KraidPartListingDefinitions
{
    /// <summary>His body, then his arm, lints, foot and fingernails.</summary>
    internal static EnemyComposite<EnemyDefinitionId> Listing { get; } = new([
        EnemyDefinitionId.Kraid, EnemyDefinitionId.KraidArm, EnemyDefinitionId.KraidLintTop, EnemyDefinitionId.KraidLintMiddle,
        EnemyDefinitionId.KraidLintBottom, EnemyDefinitionId.KraidFoot, EnemyDefinitionId.KraidFirstNail, EnemyDefinitionId.KraidSecondNail]);
}
