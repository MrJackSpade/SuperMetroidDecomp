namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// What a Ki-Hunter's wings are doing: following their body, or flying off once detached.
/// Its values are arbitrary.
/// </summary>
public enum KiHunterWingAction
{
    /// <summary>Holds the body's position, the body listed right before the wings.</summary>
    FollowBody,
    /// <summary>Flies the detached wings' arcs (<see cref="KiHunterWingState.WingMovement"/>).</summary>
    Detached,
}
