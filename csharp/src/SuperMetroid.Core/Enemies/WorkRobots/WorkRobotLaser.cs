namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The projectile definitions Work Robot fire cues spawn. The horizontal definition uses the owner's
/// signed X velocity for both directions.
/// Its values are arbitrary.
/// </summary>
internal enum WorkRobotLaser
{
    /// <summary>The up-left laser.</summary>
    UpLeft = 1,

    /// <summary>The horizontal laser.</summary>
    Horizontal,

    /// <summary>The down-left laser.</summary>
    DownLeft,

    /// <summary>The up-right laser.</summary>
    UpRight,

    /// <summary>The down-right laser.</summary>
    DownRight,
}
