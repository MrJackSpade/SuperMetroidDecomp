namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The eight ways a Yard crawls (#1472): the surface it clings to, named for the way its top faces, and the way it
/// moves along that surface. Each pair on one surface reverses into the other.
/// </summary>
public enum YardDirection
{
    /// <summary>On a wall to its left, top facing right, moving up.</summary>
    OnLeftWallMovingUp,

    /// <summary>On a wall to its left, top facing right, moving down.</summary>
    OnLeftWallMovingDown,

    /// <summary>On a wall to its right, top facing left, moving up.</summary>
    OnRightWallMovingUp,

    /// <summary>On a wall to its right, top facing left, moving down.</summary>
    OnRightWallMovingDown,

    /// <summary>On the ceiling, moving left.</summary>
    OnCeilingMovingLeft,

    /// <summary>On the ceiling, moving right.</summary>
    OnCeilingMovingRight,

    /// <summary>On the floor, moving left.</summary>
    OnFloorMovingLeft,

    /// <summary>On the floor, moving right.</summary>
    OnFloorMovingRight,
}
