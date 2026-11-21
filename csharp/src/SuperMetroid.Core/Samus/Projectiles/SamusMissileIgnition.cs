namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where a missile or Super Missile is in its ignition (#2529). A fired or reflected missile coasts until its first update,
/// then ignites: its velocity is rebuilt from its ignition speed and Samus's inherited motion, and only then do the
/// missile accelerations and a Super Missile's collision link begin. The ignited missile keeps the speed it ignited at.
/// </summary>
public abstract partial record SamusMissileIgnition
{
    /// <summary>Closes the hierarchy to the stages declared here.</summary>
    private SamusMissileIgnition()
    {
    }

    /// <summary>What a beam fired into the missile's place finds of its ignition (#2524).</summary>
    /// <param name="missile">What the missile holds besides its ignition.</param>
    internal abstract BeamFiredOverLastShot.ReplacedShot ReplacedByBeam(ReplacedMissile missile);

    /// <summary>What a missile a beam replaces holds besides its ignition.</summary>
    /// <param name="ReflectionHeadStart">The head start a reflected missile re-ignites with.</param>
    /// <param name="LinkPlace">The place a Super Missile names as its link, or null.</param>
    internal readonly record struct ReplacedMissile(SignedEightEight ReflectionHeadStart, int? LinkPlace);

    /// <summary>Whether the missile has ignited since it was fired or reflected.</summary>
    public bool HasIgnited => this is Ignited;

    /// <summary>Not yet ignited since the missile was fired or reflected: where a missile starts its ignition.</summary>
    public abstract record Unignited : SamusMissileIgnition
    {
        /// <summary>Closes the group to the two stages declared here.</summary>
        private protected Unignited()
        {
        }
    }

    /// <summary>Fired and not yet ignited; it ignites at <see cref="SamusProjectileMotionDefinitions.MissileIgnitionSpeed"/>.</summary>
    public sealed partial record Pending : Unignited
    {
        /// <inheritdoc/>
        internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam(ReplacedMissile missile) => new BeamFiredOverLastShot.ReplacedShot.UnignitedMissile();
    }

    /// <summary>Reflected by an enemy and not yet re-ignited; it re-ignites with the reflection's head start.</summary>
    public sealed partial record PendingAfterReflection : Unignited
    {
        /// <inheritdoc/>
        internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam(ReplacedMissile missile) => new BeamFiredOverLastShot.ReplacedShot.ReflectedMissile(missile.ReflectionHeadStart);
    }

    /// <summary>Ignited at <paramref name="Speed"/>.</summary>
    /// <param name="Speed">The base speed it ignited at: <see cref="SamusProjectileMotionDefinitions.MissileIgnitionSpeed"/>, or <see cref="SamusProjectileMotionDefinitions.ReflectedMissileIgnitionSpeed"/> after a reflection's head start.</param>
    public sealed partial record Ignited(SignedEightEight Speed) : SamusMissileIgnition
    {
        /// <inheritdoc/>
        internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam(ReplacedMissile missile) => new BeamFiredOverLastShot.ReplacedShot.IgnitedMissile(Speed, missile.LinkPlace);
    }
}
