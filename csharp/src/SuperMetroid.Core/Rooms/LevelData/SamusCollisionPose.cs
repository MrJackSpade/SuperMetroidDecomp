namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What room collision needs to know about the pose Samus collides in (#1395): stations admit her only after running
/// into their wall or standing still on their floor, and an elevator arms only while she stands below the running
/// poses. Samus forms it from her pose.
/// </summary>
public enum SamusCollisionPose
{
    /// <summary>Standing facing right or left in the normal pose.</summary>
    StandingNormal,

    /// <summary>Any other pose below the running poses ($00-$08).</summary>
    StandingOther,

    /// <summary>Run into a wall on the left.</summary>
    RanIntoWallLeft,

    /// <summary>Run into a wall on the right.</summary>
    RanIntoWallRight,

    /// <summary>Any other pose.</summary>
    Other,
}
