using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one of the twelve wall turrets in Mother Brain's room keeps: which turret it is, where on the eight-direction
/// ring it points, which way it turns, and its two waits.
/// </summary>
/// <param name="turret">Which of the twelve turrets this is, 0 through 11; it limits the directions the turret may face.</param>
/// <param name="ringPosition">The ring position the turret starts facing.</param>
internal sealed class MotherBrainTurretState(int turret, int ringPosition) : EnemyProjectileState
{
    /// <summary>Which of the twelve turrets this is, 0 through 11; it limits the directions the turret may face.</summary>
    internal int Turret { get; } = turret;

    /// <summary>The ring position the turret faces, kept to eight bits as the turn that produced it left it.</summary>
    internal int RingPosition
    {
        get;
        set => field = WordWrap.LowByte(value);
    } = WordWrap.LowByte(ringPosition);

    /// <summary>The ring steps the turret turns by when it next turns, one either way; it starts turning forward.</summary>
    internal int RotationStep
    {
        get;
        set => field = WordWrap.SignedLowByte(value);
    } = 1;

    /// <summary>Updates until the turret next turns; it turns on the update this reaches zero.</summary>
    internal WrappingCountdown RotationWait { get; set; }

    /// <summary>Updates until the turret next fires; it fires on the update this reaches zero.</summary>
    internal WrappingCountdown FiringWait { get; set; }
}
