namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The ten directions Samus fires a shot in.
/// The duplicated vertical directions retain Samus's facing side, which selects distinct
/// muzzle offsets even though their velocity axes match.
/// </summary>
public enum SamusProjectileDirection
{
    /// <summary>Vertical upward direction retaining right-facing muzzle placement.</summary>
    UpFacingRight,
    /// <summary>Diagonal up-right direction.</summary>
    UpRight,
    /// <summary>Horizontal right direction.</summary>
    Right,
    /// <summary>Diagonal down-right direction.</summary>
    DownRight,
    /// <summary>Vertical downward direction retaining right-facing muzzle placement.</summary>
    DownFacingRight,
    /// <summary>Vertical downward direction retaining left-facing muzzle placement.</summary>
    DownFacingLeft,
    /// <summary>Diagonal down-left direction.</summary>
    DownLeft,
    /// <summary>Horizontal left direction.</summary>
    Left,
    /// <summary>Diagonal up-left direction.</summary>
    UpLeft,
    /// <summary>Vertical upward direction retaining left-facing muzzle placement.</summary>
    UpFacingLeft,
}
