using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one of Mother Brain's following explosions keeps: where it sits from the actor it follows. A death explosion
/// follows her body and a rainbow-beam impact follows Samus; each update places the explosion at that actor's position
/// moved by this offset.
/// </summary>
/// <param name="offset">The explosion's place relative to the actor it follows, in pixels.</param>
internal sealed class MotherBrainFollowingExplosionState(Offset16<RoomPixels> offset) : EnemyProjectileState
{
    /// <summary>The explosion's place relative to the actor it follows, in pixels.</summary>
    internal Offset16<RoomPixels> Offset { get; } = offset;
}
