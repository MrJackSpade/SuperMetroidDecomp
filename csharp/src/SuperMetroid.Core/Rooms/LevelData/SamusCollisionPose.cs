namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// What room collision needs to know about the pose Samus collides in (#1395): stations admit her only after running
/// into their wall or standing still on their floor, and an elevator arms only while she stands in the power-suit front
/// view or a standing pose. Samus forms it from her pose.
/// </summary>
public enum SamusCollisionPose
{
    /// <summary>Standing facing right or left in the normal pose.</summary>
    StandingNormal,

    /// <summary>The power-suit front view or a standing pose aimed up, diagonally up or diagonally down.</summary>
    StandingOther,

    /// <summary>Run into a wall on the left.</summary>
    RanIntoWallLeft,

    /// <summary>Run into a wall on the right.</summary>
    RanIntoWallRight,

    /// <summary>Any other pose.</summary>
    Other,
}
