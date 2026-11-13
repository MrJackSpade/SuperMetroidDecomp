namespace SuperMetroid.Core.Spatial;

/// <summary>One of the eight directions of the compass on screen: the four straight ones and the four diagonals.</summary>
public enum CompassDirection
{
    /// <summary>Straight up the screen.</summary>
    Up,

    /// <summary>Up and to the right.</summary>
    UpRight,

    /// <summary>Straight to the right.</summary>
    Right,

    /// <summary>Down and to the right.</summary>
    DownRight,

    /// <summary>Straight down the screen.</summary>
    Down,

    /// <summary>Down and to the left.</summary>
    DownLeft,

    /// <summary>Straight to the left.</summary>
    Left,

    /// <summary>Up and to the left.</summary>
    UpLeft,
}
