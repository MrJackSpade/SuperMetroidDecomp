namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The eight mutually exclusive directions used by Mother Brain's room turrets.</summary>
internal enum MotherBrainTurretDirection
{
    /// <summary>Horizontal leftward turret aim and bullet launch.</summary>
    Left = 0,
    /// <summary>Diagonal aim toward the lower left.</summary>
    DownLeft = 1,
    /// <summary>Vertical downward turret aim.</summary>
    Down = 2,
    /// <summary>Diagonal aim toward the lower right.</summary>
    DownRight = 3,
    /// <summary>Horizontal rightward turret aim and bullet launch.</summary>
    Right = 4,
    /// <summary>Diagonal aim toward the upper right.</summary>
    UpRight = 5,
    /// <summary>Vertical upward turret aim.</summary>
    Up = 6,
    /// <summary>Diagonal aim toward the upper left.</summary>
    UpLeft = 7,
}
