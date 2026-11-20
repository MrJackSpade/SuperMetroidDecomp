namespace SuperMetroid.Core.Progression;

/// <summary>
/// What strikes a block's room object (#2558): one of Samus's projectiles, or the grapple beam's touch, a role of its own
/// rather than a projectile. A door, gate or shootable object asks only which projectile family the strike lands as; the
/// critters' escape wall alone tells the grapple beam's touch apart and refuses it.
/// </summary>
public abstract record BlockStrike
{
    /// <summary>Only the two strikes below derive from the strike.</summary>
    private BlockStrike()
    {
    }

    /// <summary>The projectile family the strike lands as: the projectile's own, and a beam for the grapple beam's touch.</summary>
    public abstract SamusProjectileFamily Family { get; }

    /// <summary>The strike of <paramref name="projectile"/>.</summary>
    /// <param name="projectile">The projectile that strikes.</param>
    public static BlockStrike By(SamusProjectileType projectile) => new ProjectileStrike(projectile.Family);

    /// <summary>A strike by one of Samus's projectiles.</summary>
    /// <param name="ProjectileFamily">The striking projectile's family.</param>
    public sealed record ProjectileStrike(SamusProjectileFamily ProjectileFamily) : BlockStrike
    {
        /// <inheritdoc/>
        public override SamusProjectileFamily Family => ProjectileFamily;
    }

    /// <summary>The grapple beam's touch, which lands as a beam.</summary>
    public sealed record GrappleTouch : BlockStrike
    {
        /// <summary>The grapple beam's touch; every touch strikes alike.</summary>
        public static GrappleTouch Instance { get; } = new();

        /// <summary>Only <see cref="Instance"/> is the touch.</summary>
        private GrappleTouch()
        {
        }

        /// <inheritdoc/>
        public override SamusProjectileFamily Family => SamusProjectileFamily.Beam;
    }
}
