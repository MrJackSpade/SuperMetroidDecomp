using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The cue one native enemy death projectile command becomes: a signal of its kind, or an effect-actor spawn within its
/// spread whose sprite object the command's two operand bytes name.
/// </summary>
/// <param name="Kind">The signalled kind; none for a spawning command.</param>
/// <param name="Spread">The spread a spawning command's cue carries; none for a signalling one.</param>
internal readonly record struct EnemyDeathProjectileCueRulesCommand(EnemyDeathProjectileCueKind? Kind, EnemyDeathSpriteSpread? Spread)
{
    /// <summary>The command's operand bytes: a spawning command's sprite object word, or none.</summary>
    internal int OperandBytes => Spread is null ? 0 : 2;
}
