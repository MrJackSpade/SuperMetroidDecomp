namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one of the twelve wall turrets in Mother Brain's room keeps: which turret it is, which way it points, which way
/// round it turns next, and its two waits.
/// </summary>
/// <param name="motherBrain">The encounter whose room the turret guards.</param>
/// <param name="turret">The turret this is: its placement, the direction it starts facing and the arc it may turn through.</param>
internal sealed class MotherBrainTurretState(MotherBrainEnemyState motherBrain, MotherBrainTurretDefinition turret)
    : MotherBrainProjectileState(motherBrain)
{
    /// <summary>The turret this is: its placement and the arc of directions it may face.</summary>
    internal MotherBrainTurretDefinition Turret { get; } = turret;

    /// <summary>The direction the turret points and fires in; its turns keep it inside its arc (#1950).</summary>
    internal MotherBrainTurretDirection Direction { get; set; } = turret.InitialDirection;

    /// <summary>
    /// Which way round the turret turns when it next turns, one direction at a time: it starts turning counterclockwise,
    /// from left toward down, and turns back the other way when the next direction lies outside its arc.
    /// </summary>
    internal RotationDirection Turning { get; set; } = RotationDirection.Counterclockwise;

    /// <summary>The turret's wait before it next turns, which it turns on the update of running out (#2469).</summary>
    internal BossCountdown RotationWait { get; } = new();

    /// <summary>The turret's wait before it next fires, which it fires on the update of running out (#2469).</summary>
    internal BossCountdown FiringWait { get; } = new();
}
