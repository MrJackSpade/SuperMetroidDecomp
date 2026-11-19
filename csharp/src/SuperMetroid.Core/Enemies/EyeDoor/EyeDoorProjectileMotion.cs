namespace SuperMetroid.Core.Enemies.EyeDoor;

/// <summary>
/// How an eye door's shot or sweat drop moves itself (#1843).
/// </summary>
internal enum EyeDoorProjectileMotion
{
    /// <summary>
    /// Flies an eye-door shot with block collision, accumulating its angle-selected acceleration, and starts its
    /// explosion on impact or once its door opens.
    /// </summary>
    ShotFlying,

    /// <summary>Drops an eye-door sweat drop, its fall speeding up by three sixty-fourths of a pixel an update each update, and splashes
    /// on the floor.</summary>
    SweatFalling,
}
