namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Samus's vertical direction. Her vertical speed is a magnitude, and this is its direction: she rises, falls or holds
/// still. Its values are arbitrary.
/// </summary>
public enum SamusVerticalDirection
{
    /// <summary>No vertical movement (grounded, or a pose change that cleared the motion).</summary>
    None,

    /// <summary>Moving up (jumping, bouncing, rising).</summary>
    Up,

    /// <summary>Moving down (falling).</summary>
    Down,
}
