namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>Which of its eight directional loops an Sbug shows, chosen from the heading it moves on.</summary>
public enum SbugFacing
{
    /// <summary>Facing right.</summary>
    Right,

    /// <summary>Facing up and to the right.</summary>
    UpRight,

    /// <summary>Facing up.</summary>
    Up,

    /// <summary>Facing up and to the left.</summary>
    UpLeft,

    /// <summary>Facing left.</summary>
    Left,

    /// <summary>Facing down and to the left.</summary>
    DownLeft,

    /// <summary>Facing down.</summary>
    Down,

    /// <summary>Facing down and to the right.</summary>
    DownRight,
}
