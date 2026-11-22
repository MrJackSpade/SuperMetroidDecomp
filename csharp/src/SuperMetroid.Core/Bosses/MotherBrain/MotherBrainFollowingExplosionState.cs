using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one of Mother Brain's rainbow-beam impacts keeps: where it sits from Samus, whom it follows; each update places
/// the explosion at her position moved by this offset. A death explosion follows Mother Brain's body
/// (<see cref="MotherBrainBodyExplosionState"/>).
/// </summary>
/// <param name="offset">The explosion's place relative to Samus, in pixels.</param>
internal sealed partial class MotherBrainFollowingExplosionState(Offset16<RoomPixels> offset) : EnemyProjectileState
{
    /// <summary>The explosion's place relative to Samus, in pixels.</summary>
    internal Offset16<RoomPixels> Offset { get; } = offset;
}
