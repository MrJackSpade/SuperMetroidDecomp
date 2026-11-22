namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one of Mother Brain's death explosions keeps: the encounter whose body it follows and where it sits from that
/// body; each update places the explosion at her body's position moved by this offset.
/// </summary>
/// <param name="motherBrain">The encounter whose body the explosion follows.</param>
/// <param name="offset">The explosion's place relative to her body, in pixels.</param>
internal sealed partial class MotherBrainBodyExplosionState(MotherBrainEnemyState motherBrain, Offset16<RoomPixels> offset)
    : MotherBrainProjectileState(motherBrain)
{
    /// <summary>The explosion's place relative to her body, in pixels.</summary>
    internal Offset16<RoomPixels> Offset { get; } = offset;
}
