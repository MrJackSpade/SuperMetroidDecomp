namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The direction a Cacatac spike flies, and for the sideways spikes which way the spike faces: each direction names the
/// spike's animation (<see cref="CacatacProjectileAnimations"/>).
/// </summary>
public enum CacatacSpikeDirection
{
    /// <summary>Moves left at <see cref="CacatacProjectileDefinitions.CardinalSpeed"/>, an upward-facing horizontal spike.</summary>
    LeftFacingUp,
    /// <summary>Moves up and left at <see cref="CacatacProjectileDefinitions.DiagonalSpeed"/> on each axis.</summary>
    UpLeft,
    /// <summary>Moves straight up at <see cref="CacatacProjectileDefinitions.CardinalSpeed"/>.</summary>
    Up,
    /// <summary>Moves up and right at <see cref="CacatacProjectileDefinitions.DiagonalSpeed"/> on each axis.</summary>
    UpRight,
    /// <summary>Moves right at <see cref="CacatacProjectileDefinitions.CardinalSpeed"/>, an upward-facing horizontal spike.</summary>
    RightFacingUp,
    /// <summary>Moves left at <see cref="CacatacProjectileDefinitions.CardinalSpeed"/>, a downward-facing horizontal spike.</summary>
    LeftFacingDown,
    /// <summary>Moves down and left at <see cref="CacatacProjectileDefinitions.DiagonalSpeed"/> on each axis.</summary>
    DownLeft,
    /// <summary>Moves straight down at <see cref="CacatacProjectileDefinitions.CardinalSpeed"/>.</summary>
    Down,
    /// <summary>Moves down and right at <see cref="CacatacProjectileDefinitions.DiagonalSpeed"/> on each axis.</summary>
    DownRight,
    /// <summary>Moves right at <see cref="CacatacProjectileDefinitions.CardinalSpeed"/>, a downward-facing horizontal spike.</summary>
    RightFacingDown,
}
