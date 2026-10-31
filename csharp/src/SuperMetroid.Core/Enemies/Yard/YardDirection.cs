namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// The eight ways a Yard crawls (#1472): the surface it clings to, named for the way its top faces, and the way it
/// moves along that surface. Each pair on one surface reverses into the other.
/// </summary>
public enum YardDirection
{
    /// <summary>On a wall to its left, top facing right, moving up.</summary>
    UpsideRightMovingUp,

    /// <summary>On a wall to its left, top facing right, moving down.</summary>
    UpsideRightMovingDown,

    /// <summary>On a wall to its right, top facing left, moving up.</summary>
    UpsideLeftMovingUp,

    /// <summary>On a wall to its right, top facing left, moving down.</summary>
    UpsideLeftMovingDown,

    /// <summary>On the ceiling, moving left.</summary>
    UpsideDownMovingLeft,

    /// <summary>On the ceiling, moving right.</summary>
    UpsideDownMovingRight,

    /// <summary>On the floor, moving left.</summary>
    UpsideUpMovingLeft,

    /// <summary>On the floor, moving right.</summary>
    UpsideUpMovingRight,
}
