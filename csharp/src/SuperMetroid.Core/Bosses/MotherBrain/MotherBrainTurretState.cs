using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one of the twelve wall turrets in Mother Brain's room keeps: which turret it is, where on the eight-direction
/// ring it points, which way it turns, and its two waits.
/// </summary>
/// <param name="turret">The turret this is: its placement and the arc of directions it may face.</param>
/// <param name="ringPosition">The ring position the turret starts facing.</param>
internal sealed class MotherBrainTurretState(MotherBrainTurretDefinition turret, int ringPosition) : EnemyProjectileState
{
    /// <summary>The turret this is: its placement and the arc of directions it may face.</summary>
    internal MotherBrainTurretDefinition Turret { get; } = turret;

    /// <summary>The ring position the turret faces, 0 through 7; its turns keep it inside its allowed sector.</summary>
    internal int RingPosition { get; set; } = ringPosition;

    /// <summary>The ring steps the turret turns by when it next turns, one either way; it starts turning forward.</summary>
    internal int RotationStep { get; set; } = 1;

    /// <summary>Updates until the turret next turns; it turns on the update this reaches zero.</summary>
    internal int RotationWait { get; set; }

    /// <summary>Updates until the turret next fires; it fires on the update this reaches zero.</summary>
    internal int FiringWait { get; set; }
}
