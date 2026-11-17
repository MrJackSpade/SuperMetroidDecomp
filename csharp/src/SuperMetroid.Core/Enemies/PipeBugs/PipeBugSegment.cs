namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The Zeb, Zebbo, Gamet and Geega Pipe Bugs' rising, shooting, flying and arcing loops (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="PipeBugAnimations"/>.
/// </summary>
public enum PipeBugSegment
{
    /// <summary>
    /// A Zeb facing left rising from its pipe, its wings beating from up to down and back, repeating.
    /// </summary>
    ZebRisingLeft,
    /// <summary>A Zeb facing left shooting across, its wings beating twice as fast, repeating.</summary>
    ZebShootingLeft,
    /// <summary>
    /// A Zeb facing right rising from its pipe, its wings beating from up to down and back, repeating.
    /// </summary>
    ZebRisingRight,
    /// <summary>A Zeb facing right shooting across, its wings beating twice as fast, repeating.</summary>
    ZebShootingRight,
    /// <summary>A Zebbo facing left rising from its pipe, its wings beating up, level and down, repeating.</summary>
    ZebboRisingLeft,
    /// <summary>A Zebbo darting left, its wings beating up, level and down, repeating.</summary>
    ZebboShootingLeft,
    /// <summary>A Zebbo facing right rising from its pipe, its wings beating up, level and down, repeating.</summary>
    ZebboRisingRight,
    /// <summary>A Zebbo darting right, its wings beating up, level and down, repeating.</summary>
    ZebboShootingRight,
    /// <summary>
    /// A Gamet facing left rising from its pipe, its wings beating from up to down and back, repeating.
    /// </summary>
    GametRisingLeft,
    /// <summary>A Gamet facing left flying across, its wings beating twice as fast, repeating.</summary>
    GametFlyingLeft,
    /// <summary>
    /// A Gamet facing right rising from its pipe, its wings beating from up to down and back, repeating.
    /// </summary>
    GametRisingRight,
    /// <summary>A Gamet facing right flying across, its wings beating twice as fast, repeating.</summary>
    GametFlyingRight,
    /// <summary>A Geega flying level facing left, its wings beating up, level and down, repeating.</summary>
    GeegaFlyingLeft,
    /// <summary>A Geega tilted facing left as it arcs away, its wings beating quickly, repeating.</summary>
    GeegaArcingLeft,
    /// <summary>A Geega flying level facing right, its wings beating up, level and down, repeating.</summary>
    GeegaFlyingRight,
    /// <summary>A Geega tilted facing right as it arcs away, its wings beating quickly, repeating.</summary>
    GeegaArcingRight,
}
