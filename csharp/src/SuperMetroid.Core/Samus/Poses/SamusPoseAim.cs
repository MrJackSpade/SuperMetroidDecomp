using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What a pose does with Samus's arm cannon. An aiming pose fires in one of the ten projectile directions and places a
/// held charge's flare at that direction's muzzle. A turning pose fires nothing and places the flare at its turning
/// placement. A pose that disables shooting fires nothing, draws no flare, cancels a fired Grapple and lands in the
/// ordinary facing landing. An unauthored pose
/// (<see cref="Compatibility.UnauthoredPoseReads{TPose, TDefinition}"/>) fires nothing and places the flare as its
/// definition says.
/// </summary>
internal readonly record struct SamusPoseAim
{
    /// <summary>Keeps the aim.</summary>
    /// <param name="direction">The direction the pose fires in, or null.</param>
    /// <param name="flare">Where the pose places a held charge's flare, or null when it disables shooting.</param>
    private SamusPoseAim(SamusProjectileDirection? direction, SamusFlareAim? flare)
    {
        Direction = direction;
        Flare = flare;
    }

    /// <summary>The direction the pose fires in, or null for a pose that fires nothing.</summary>
    public SamusProjectileDirection? Direction { get; }

    /// <summary>Where the pose places a held charge's flare, or null when the pose disables shooting.</summary>
    public SamusFlareAim? Flare { get; }

    /// <summary>Whether the pose disables shooting: it fires nothing and draws no flare.</summary>
    public bool DisablesShooting => Flare is null;

    /// <summary>Whether the pose aims straight up, facing either way.</summary>
    public bool AimsStraightUp => Direction is SamusProjectileDirection.UpFacingRight or SamusProjectileDirection.UpFacingLeft;

    /// <summary>A pose aiming in <paramref name="direction"/>.</summary>
    public static SamusPoseAim Aiming(SamusProjectileDirection direction) => new(direction, SamusFlareAim.AtMuzzle(direction));

    /// <summary>A pose turning while it holds <paramref name="aim"/>.</summary>
    public static SamusPoseAim Turning(SamusTurningAim aim) => Unaimed(SamusFlareAim.WhileTurning(aim));

    /// <summary>A pose that fires nothing and places a held charge's flare at <paramref name="flare"/>.</summary>
    public static SamusPoseAim Unaimed(SamusFlareAim flare) => new(null, flare);

    /// <summary>A pose that disables shooting.</summary>
    public static SamusPoseAim ShootingDisabled { get; } = new(null, null);
}
