namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The lasers Work Robot fire cues spawn (#2278): each flies ahead of the robot, the way it faces, and up, straight or
/// down. Its values are arbitrary.
/// </summary>
internal enum WorkRobotLaser
{
    /// <summary>The laser that flies up and ahead.</summary>
    Upward,

    /// <summary>The laser that flies straight ahead.</summary>
    Horizontal,

    /// <summary>The laser that flies down and ahead.</summary>
    Downward,
}
