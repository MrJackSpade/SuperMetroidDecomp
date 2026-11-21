namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A missile's ignition (#2346), created when the missile is fired or reflected; a Super Missile and its link keep theirs
/// with the link they name (<see cref="SuperMissileShotState"/>).
/// </summary>
internal class MissileShotState : SamusShotKindState
{
    /// <summary>Starts a missile's ignition pending.</summary>
    /// <param name="ignition">Where the missile starts in its ignition: pending when fired, pending after a reflection when reflected.</param>
    internal MissileShotState(SamusMissileIgnition.Unignited ignition) => Ignition = Ensure.NotNull(ignition);

    /// <summary>Where the missile is in its ignition, with the speed it ignited at once it has.</summary>
    internal SamusMissileIgnition Ignition { get; private set; }

    /// <summary>The place whose projectile the missile moves as its link once ignited; none for an ordinary missile.</summary>
    private protected virtual int? IgnitedLinkPlace => null;

    /// <summary>
    /// A missile awaiting its first ignition, a reflected one awaiting its re-ignition with the head start its reflection
    /// gives, or an ignited one with its speed and the link it moves.
    /// </summary>
    /// <exception cref="InvalidOperationException">The missile is in an undefined ignition.</exception>
    internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam => Ignition switch
    {
        SamusMissileIgnition.Pending => new BeamFiredOverLastShot.ReplacedShot.UnignitedMissile(),
        SamusMissileIgnition.PendingAfterReflection => new BeamFiredOverLastShot.ReplacedShot.ReflectedMissile(ReflectionHeadStart),
        SamusMissileIgnition.Ignited ignited => new BeamFiredOverLastShot.ReplacedShot.IgnitedMissile(ignited.Speed, IgnitedLinkPlace),
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusMissileIgnition)} {Ignition}."),
    };

    /// <summary>
    /// The head start a reflection gives a missile toward the speed it re-ignites at: what that speed exceeds a fired
    /// missile's by.
    /// </summary>
    private static SignedEightEight ReflectionHeadStart =>
        SamusProjectileMotionDefinitions.ReflectedMissileIgnitionSpeed - SamusProjectileMotionDefinitions.MissileIgnitionSpeed;

    /// <summary>Ignites the missile at <paramref name="speed"/>.</summary>
    /// <param name="speed">The base speed it ignites at: one pixel an update, or more after a reflection's head start.</param>
    internal void Ignite(SignedEightEight speed) => Ignition = new SamusMissileIgnition.Ignited(speed);

    /// <inheritdoc/>
    internal sealed override bool AwaitsIgnitionAsFired => Ignition.AwaitsIgnitionAsFired;

    /// <inheritdoc/>
    internal override int? NamedLinkPlace => null;

    /// <inheritdoc/>
    internal sealed override bool DeletesItselfAfterEnemyHit => false;

    /// <inheritdoc/>
    internal override BeamFiredOverLastShot.ReplacedShot ReplacedByBeam => Ignition.ReplacedByBeam(linkPlace: null);
}
