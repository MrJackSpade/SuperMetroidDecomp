namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// Yard's movement behaviour.
/// Its values are arbitrary.
/// </summary>
public enum YardMovementFunction
{
    /// <summary>Empty behaviour: main movement remains idle until an animation cue installs a movement behaviour.</summary>
    AwaitingAnimationCue = 1,
    /// <summary>Probes the current attachment surface while hidden and drops Yard if support disappears.</summary>
    Hiding,
    /// <summary>Floor-attached leftward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideUpMovingLeft,
    /// <summary>Downward crawl with Yard's top facing left, using vertical surface and corner probes.</summary>
    CrawlingUpsideLeftMovingDown,
    /// <summary>Ceiling-attached rightward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideDownMovingRight,
    /// <summary>Upward crawl with Yard's top facing right, using vertical surface and corner probes.</summary>
    CrawlingUpsideRightMovingUp,
    /// <summary>Floor-attached rightward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideUpMovingRight,
    /// <summary>Downward crawl with Yard's top facing right, using vertical surface and corner probes.</summary>
    CrawlingUpsideRightMovingDown,
    /// <summary>Ceiling-attached leftward crawl using horizontal surface and corner probes.</summary>
    CrawlingUpsideDownMovingLeft,
    /// <summary>Upward crawl with Yard's top facing left, using vertical surface and corner probes.</summary>
    CrawlingUpsideLeftMovingUp,
    /// <summary>Applies 16.16 shell motion, horizontal decay, gravity, per-axis collision bounces, and low-speed floor landing.</summary>
    Airborne,
}
