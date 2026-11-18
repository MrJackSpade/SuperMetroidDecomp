namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The state of a projectile that flickers (#1839, #2455): its family hides it on the updates its flicker chooses, and it
/// is drawn only while shown. The n00b tube's crack and shards flicker so.
/// </summary>
internal interface IFlickeringProjectileState
{
    /// <summary>Whether the projectile's flicker hides it on this update.</summary>
    bool IsHidden { get; }
}
