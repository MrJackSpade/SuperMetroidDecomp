using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>What a n00b-tube crack keeps: whether its flicker hides it, which it keeps once it falls (#2455).</summary>
internal sealed class NoobTubeCrackState : EnemyProjectileState
{
    /// <summary>Whether the crack's flicker hides it on this update; the last choice stays once the crack falls.</summary>
    public bool IsHidden { get; internal set; }
}
