namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The direction a Cacatac spike flies, and for the sideways spikes which way the spike faces: each direction names the
/// spike's animation (<see cref="CacatacProjectileAnimations"/>).
/// </summary>
public enum CacatacSpikeDirection
{
    /// <summary>Moves left two pixels per projectile update, an upward-facing horizontal spike.</summary>
    LeftFacingUp,
    /// <summary>Moves up and left by 1.5 pixels on each axis per projectile update.</summary>
    UpLeft,
    /// <summary>Moves straight up two pixels per projectile update.</summary>
    Up,
    /// <summary>Moves up and right by 1.5 pixels on each axis per projectile update.</summary>
    UpRight,
    /// <summary>Moves right two pixels per projectile update, an upward-facing horizontal spike.</summary>
    RightFacingUp,
    /// <summary>Moves left two pixels per projectile update, a downward-facing horizontal spike.</summary>
    LeftFacingDown,
    /// <summary>Moves down and left by 1.5 pixels on each axis per projectile update.</summary>
    DownLeft,
    /// <summary>Moves straight down two pixels per projectile update.</summary>
    Down,
    /// <summary>Moves down and right by 1.5 pixels on each axis per projectile update.</summary>
    DownRight,
    /// <summary>Moves right two pixels per projectile update, a downward-facing horizontal spike.</summary>
    RightFacingDown,
}
