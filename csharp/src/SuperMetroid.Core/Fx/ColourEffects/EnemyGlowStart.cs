namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// A glow an enemy's placement started (#1733), for the room's colour effects to run: the effect, and the enemy it shows on,
/// which the glow keeps (#1783).
/// </summary>
/// <param name="Glow">The glow's colour effect.</param>
/// <param name="Enemy">The enemy that started the glow and shows its frames.</param>
public sealed record EnemyGlowStart(ColourEffectId Glow, IGlowingEnemy Enemy);
