namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Ten valid direction-table indices stored in a projectile direction word's low nibble.
/// The duplicated vertical directions retain Samus's facing side, which selects distinct
/// muzzle offsets even though their velocity axes match.
/// </summary>
public enum SamusProjectileDirection : byte
{
    /// <summary>Vertical upward direction retaining right-facing muzzle placement.</summary>
    UpFacingRight = 0,
    /// <summary>Diagonal up-right direction.</summary>
    UpRight = 1,
    /// <summary>Horizontal right direction.</summary>
    Right = 2,
    /// <summary>Diagonal down-right direction.</summary>
    DownRight = 3,
    /// <summary>Vertical downward direction retaining right-facing muzzle placement.</summary>
    DownFacingRight = 4,
    /// <summary>Vertical downward direction retaining left-facing muzzle placement.</summary>
    DownFacingLeft = 5,
    /// <summary>Diagonal down-left direction.</summary>
    DownLeft = 6,
    /// <summary>Horizontal left direction.</summary>
    Left = 7,
    /// <summary>Diagonal up-left direction.</summary>
    UpLeft = 8,
    /// <summary>Vertical upward direction retaining left-facing muzzle placement.</summary>
    UpFacingLeft = 9,
}
