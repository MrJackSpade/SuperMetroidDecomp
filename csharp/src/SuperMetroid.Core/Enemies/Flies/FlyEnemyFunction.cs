namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// The routine a Mellow-family enemy's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum FlyEnemyFunction
{
    /// <summary>Circles with increasing table offsets, switching direction on wrap or attacking when cooldown is zero and Samus is within 112 horizontal pixels.</summary>
    ClockwiseCircle = 1,
    /// <summary>Circles with decreasing table offsets and the same proximity/cooldown admission as the clockwise state.</summary>
    AntiClockwiseCircle,
    /// <summary>Advances the sampled attack velocity until crossing the saved Samus Y, then reverses vertical velocity and retreats.</summary>
    AttackSamus,
    /// <summary>Continues with reversed Y velocity until the accumulated attack duration counts down through signed underflow, then starts a 24-update idle cooldown.</summary>
    Retreat,
}
