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

    /// <summary>Whether the missile has ignited since it was fired or reflected.</summary>
    public bool HasIgnited => this is Ignited;

    /// <summary>Whether the missile waits to ignite as it was fired, not after a reflection.</summary>
    internal abstract bool AwaitsIgnitionAsFired { get; }

    /// <summary>What a beam fired into the missile's place finds there (<see cref="BeamFiredOverLastShot"/>).</summary>
    /// <param name="linkPlace">The place an ignited Super Missile names as its link, or null for a missile or a Super Missile naming none.</param>
    internal abstract BeamFiredOverLastShot.ReplacedShot ReplacedByBeam(int? linkPlace);

    /// <summary>
    /// A stage before the missile's ignition, which a missile starts in when it is fired or reflected (#2554): it ignites
    /// on its update, never before.
    /// </summary>
    public abstract partial record Unignited : SamusMissileIgnition
    {
    }

    /// <summary>Fired and not yet ignited; it ignites at <see cref="SamusProjectileMotionDefinitions.MissileIgnitionSpeed"/>.</summary>
    public sealed partial record Pending : Unignited
    {
        /// <inheritdoc/>
        internal override bool AwaitsIgnitionAsFired => true;

        /// <inheritdoc/>
        internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam(int? linkPlace) =>
            new BeamFiredOverLastShot.ReplacedShot.UnignitedMissile();
    }

    /// <summary>Reflected by an enemy and not yet re-ignited; it re-ignites with the reflection's head start.</summary>
    public sealed partial record PendingAfterReflection : Unignited
    {
        /// <summary>
        /// The head start a reflection gives a missile toward the speed it re-ignites at: what that speed exceeds a fired
        /// missile's by.
        /// </summary>
        private static SignedEightEight HeadStart =>
            SamusProjectileMotionDefinitions.ReflectedMissileIgnitionSpeed - SamusProjectileMotionDefinitions.MissileIgnitionSpeed;

        /// <inheritdoc/>
        internal override bool AwaitsIgnitionAsFired => false;

        /// <inheritdoc/>
        internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam(int? linkPlace) =>
            new BeamFiredOverLastShot.ReplacedShot.ReflectedMissile(HeadStart);
    }

    /// <summary>Ignited at <paramref name="Speed"/>.</summary>
    /// <param name="Speed">The base speed it ignited at: <see cref="SamusProjectileMotionDefinitions.MissileIgnitionSpeed"/>, or <see cref="SamusProjectileMotionDefinitions.ReflectedMissileIgnitionSpeed"/> after a reflection's head start.</param>
    public sealed partial record Ignited(SignedEightEight Speed) : SamusMissileIgnition
    {
        /// <inheritdoc/>
        internal override bool AwaitsIgnitionAsFired => false;

        /// <inheritdoc/>
        internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam(int? linkPlace) =>
            new BeamFiredOverLastShot.ReplacedShot.IgnitedMissile(Speed, linkPlace);
    }
}
