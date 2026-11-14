namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>A frame an enemy's glow showed during an update (#1783), for the enemy that started the glow to show.</summary>
/// <param name="Enemy">The enemy that started the glow.</param>
/// <param name="Frame">The frame the glow showed.</param>
internal readonly record struct ShownEnemyGlow(IGlowingEnemy Enemy, ColourEffectFrame Frame);
