namespace SuperMetroid.Core.Game;

/// <summary>
/// Even byte offsets used by both bank-$A2's spike spawn command and bank-$86's ten-entry
/// instruction/movement tables. Values are not flags and must remain even.
/// </summary>
public enum CacatacSpikeDirection : ushort
{
    /// <summary>Selector $00: moves left two pixels per projectile update with the upward-facing horizontal spike program at $86:D92E.</summary>
    LeftFacingUp = 0x00,
    /// <summary>Selector $02: moves straight up two pixels per projectile update with the upward spike program at $86:D93A.</summary>
    Up = 0x02,
    /// <summary>Selector $04: moves right two pixels per projectile update with the upward-facing horizontal spike program at $86:D946.</summary>
    RightFacingUp = 0x04,
    /// <summary>Selector $06: moves left two pixels per projectile update with the downward-facing horizontal spike program at $86:D94C.</summary>
    LeftFacingDown = 0x06,
    /// <summary>Selector $08: moves straight down two pixels per projectile update with the downward spike program at $86:D958.</summary>
    Down = 0x08,
    /// <summary>Selector $0A: moves right two pixels per projectile update with the downward-facing horizontal spike program at $86:D964, natively named <c>InstList_EnemyProjectile_CacatacSpike_Down_FacingRight</c>.</summary>
    RightFacingDown = 0x0a,
    /// <summary>Selector $0C: moves up and left by 1.5 pixels on each axis per projectile update, using the diagonal spike program at $86:D934.</summary>
    UpLeft = 0x0c,
    /// <summary>Selector $0E: moves up and right by 1.5 pixels on each axis per projectile update, using the diagonal spike program at $86:D940.</summary>
    UpRight = 0x0e,
    /// <summary>Selector $10: moves down and left by 1.5 pixels on each axis per projectile update, using the diagonal spike program at $86:D952.</summary>
    DownLeft = 0x10,
    /// <summary>Selector $12: moves down and right by 1.5 pixels on each axis per projectile update, using the diagonal spike program at $86:D95E.</summary>
    DownRight = 0x12,
}
