namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// The behaviour a Mellow-family enemy's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum FlyAction
{
    /// <summary>
    /// Circles the way <see cref="FlyEnemyState.Rotation"/> names (#2277), its flight angle increasing clockwise and
    /// decreasing counterclockwise, switching direction after a full turn or attacking when cooldown is zero and Samus is
    /// within 112 horizontal pixels.
    /// </summary>
    Circling,
    /// <summary>Advances the sampled attack velocity until crossing the saved Samus Y, then reverses vertical velocity and retreats.</summary>
    AttackSamus,
    /// <summary>Continues with reversed Y velocity for the dive's updates and one more, then starts a 24-update idle cooldown.</summary>
    Retreat,
}
