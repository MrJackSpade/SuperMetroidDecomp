namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// Yard's movement behaviour.
/// Its values are arbitrary.
/// </summary>
public enum YardMovement
{
    /// <summary>
    /// Holds still: placed in the room, turning a corner or landing, the Yard moves by none of its own movements until its
    /// animation begins a crawl or a hide.
    /// </summary>
    HoldingStill,
    /// <summary>Probes the current attachment surface while hidden and drops Yard if support disappears.</summary>
    Hiding,
    /// <summary>
    /// Crawls along its surface the way <see cref="YardEnemyState.Crawl"/> names (#2278), with surface and corner probes
    /// along that surface: vertical ones on a wall, horizontal ones on a floor or ceiling.
    /// </summary>
    Crawling,
    /// <summary>Applies shell motion, horizontal decay, gravity, per-axis collision bounces, and low-speed floor landing.</summary>
    Airborne,
}
