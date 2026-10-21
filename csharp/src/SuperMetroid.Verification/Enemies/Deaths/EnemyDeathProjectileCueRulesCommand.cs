using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Verification.Enemies.Deaths;

/// <summary>The kind and operand bytes of one enemy death projectile cue rules command.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="OperandBytes">The operand bytes.</param>
internal readonly record struct EnemyDeathProjectileCueRulesCommand(EnemyDeathProjectileCueKind Kind, int OperandBytes);
