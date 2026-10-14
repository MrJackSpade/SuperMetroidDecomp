namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The three operations reachable from the two Ceres arrival programs.</summary>
internal enum CeresElevatorProjectileOperation
{
    /// <summary>Publishes a timed spritemap pose and advances to the next program word.</summary>
    Frame,
    /// <summary>Releases the projectile after the elevator landing sequence.</summary>
    Delete,
    /// <summary>Loops the pad or platform animation to its first frame.</summary>
    Goto,
}
