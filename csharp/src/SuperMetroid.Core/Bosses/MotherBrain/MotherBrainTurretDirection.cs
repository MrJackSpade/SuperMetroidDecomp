namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The eight directions a wall turret in Mother Brain's room can point and fire in; their order around the compass is
/// <see cref="MotherBrainTurretDefinitions.Ring"/>.
/// </summary>
internal enum MotherBrainTurretDirection
{
    /// <summary>Points and fires to the left.</summary>
    Left,
    /// <summary>Points and fires down and to the left.</summary>
    DownLeft,
    /// <summary>Points and fires straight down.</summary>
    Down,
    /// <summary>Points and fires down and to the right.</summary>
    DownRight,
    /// <summary>Points and fires to the right.</summary>
    Right,
    /// <summary>Points and fires up and to the right.</summary>
    UpRight,
    /// <summary>Points and fires straight up.</summary>
    Up,
    /// <summary>Points and fires up and to the left.</summary>
    UpLeft,
}
