namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The Work Robot's laser animations (#641): the flare a spawn enters and the ring it then loops (#2276), in authored order.
/// Their steps are <see cref="WorkRobotLaserAnimations"/>.
/// </summary>
internal enum WorkRobotLaserSegment
{
    /// <summary>The laser's opening flare: a glint swelling into a ball.</summary>
    Flare,
    /// <summary>The ring the flare opens into, looped.</summary>
    Ring,
}
