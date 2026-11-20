using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What a pose does with Samus's arm cannon (#2529). An aiming pose fires in one of the ten projectile directions and places
/// a held charge's flare at that direction's muzzle. A turning pose fires nothing and places the flare at its turning
/// placement. A pose that disables shooting fires nothing, draws no flare, cancels a fired Grapple and lands in the
/// ordinary facing landing.
/// </summary>
internal abstract record SamusPoseAim
{
    /// <summary>Closes the hierarchy to the aims declared here.</summary>
    private SamusPoseAim()
    {
    }

    /// <summary>Whether the pose disables shooting: it fires nothing and draws no flare.</summary>
    public bool DisablesShooting => this is ShootingDisabled;

    /// <summary>Whether the pose aims straight up, facing either way.</summary>
    public bool AimsStraightUp => this is Aiming { Direction: var direction } && SamusProjectileDirectionDefinitions.Axes(direction).IsStraightUp;

    /// <summary>
    /// A pose that holds its aim, not turning: it aims or disables shooting. A jump or fall in such a pose lands keeping
    /// it; a turning pose turns instead of landing.
    /// </summary>
    internal interface ISteady
    {
        /// <summary>The landing a jump or fall in the pose takes, facing left or right, with Shoot held or released.</summary>
        /// <param name="facingLeft">Whether she faces left.</param>
        /// <param name="shotHeld">Whether Shoot is held.</param>
        SamusPoseId Landing(bool facingLeft, bool shotHeld);
    }

    /// <summary>A pose whose arm cannon is armed: it aims or turns, and places a held charge's flare.</summary>
    internal abstract record Armed : SamusPoseAim
    {
        /// <summary>Where the pose places a held charge's flare.</summary>
        public abstract SamusFlareAim Flare { get; }
    }

    /// <summary>A pose aiming in <paramref name="Direction"/>, which places a held charge's flare at that direction's muzzle.</summary>
    /// <param name="Direction">The direction the pose fires in.</param>
    internal sealed record Aiming(SamusProjectileDirection Direction) : Armed, ISteady
    {
        /// <inheritdoc/>
        public override SamusFlareAim Flare { get; } = new SamusFlareAim.AtMuzzle(Direction);

        /// <summary>
        /// The aimed landing keeping <see cref="Direction"/>; a level aim takes the landing's extra Shot-binding test, held
        /// Shot selecting the firing landing.
        /// </summary>
        /// <param name="facingLeft">Whether she faces left; the aim names the way.</param>
        /// <param name="shotHeld">Whether Shoot is held.</param>
        public SamusPoseId Landing(bool facingLeft, bool shotHeld)
        {
            SamusLandingChoice landing = SamusAimedPoseChoiceDefinitions.Landings[Direction];
            return shotHeld ? landing.ShootHeld : landing.ShootReleased;
        }
    }

    /// <summary>
    /// A pose turning while it holds <paramref name="HeldAim"/>: it fires nothing and places a held charge's flare at that
    /// aim's turning placement.
    /// </summary>
    /// <param name="HeldAim">The aim the turn holds.</param>
    internal sealed record Turning(SamusTurningAim HeldAim) : Armed
    {
        /// <inheritdoc/>
        public override SamusFlareAim Flare { get; } = new SamusFlareAim.WhileTurning(HeldAim);
    }

    /// <summary>A pose that disables shooting; it lands in the ordinary landing facing its way.</summary>
    internal sealed record ShootingDisabled : SamusPoseAim, ISteady
    {
        /// <inheritdoc/>
        public SamusPoseId Landing(bool facingLeft, bool shotHeld) =>
            facingLeft ? SamusPoseId.NormalLandingLeftPose : SamusPoseId.NormalLandingRightPose;
    }
}
