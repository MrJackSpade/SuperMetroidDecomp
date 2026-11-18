namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A missile's ignition (#2346), created when the missile is fired or reflected; a Super Missile and its link keep theirs
/// with the link they name (<see cref="SuperMissileShotState"/>).
/// </summary>
internal class MissileShotState : SamusShotKindState
{
    /// <summary>Starts a missile's ignition pending.</summary>
    /// <param name="ignition">Where the missile starts in its ignition: pending when fired, pending after a reflection when reflected.</param>
    /// <exception cref="ArgumentOutOfRangeException">The missile would start ignited, with no speed it ignited at.</exception>
    internal MissileShotState(SamusMissileIgnition ignition) =>
        Ignition = ignition is SamusMissileIgnition.Pending or SamusMissileIgnition.PendingAfterReflection
            ? ignition
            : throw new ArgumentOutOfRangeException(nameof(ignition), ignition, "A missile starts its ignition pending; it ignites on its update.");

    /// <summary>Where the missile is in its ignition.</summary>
    internal SamusMissileIgnition Ignition { get; private set; }

    /// <summary>The speed the missile last ignited at, or null while it has not ignited since it was fired or reflected.</summary>
    internal SignedEightEight? IgnitionSpeed { get; private set; }

    /// <summary>Ignites the missile at <paramref name="speed"/>.</summary>
    /// <param name="speed">The base speed it ignites at: one pixel an update, or more after a reflection's head start.</param>
    internal void Ignite(SignedEightEight speed)
    {
        Ignition = SamusMissileIgnition.Ignited;
        IgnitionSpeed = speed;
    }

    /// <summary>
    /// The count the missile leaves (<see cref="BeamFiredOverLastShot"/>): nothing before it first ignites, the head start
    /// its reflection gave it while it waits to re-ignite, and once ignited the speed it ignited at, in 256ths of a pixel.
    /// </summary>
    internal override int FuseLeftForBeamFiredOver => Ignition switch
    {
        SamusMissileIgnition.Pending => 0,
        SamusMissileIgnition.PendingAfterReflection =>
            (SamusProjectileMotionDefinitions.ReflectedMissileIgnitionSpeed - SamusProjectileMotionDefinitions.MissileIgnitionSpeed)
                .InTwoHundredFiftySixthsOfPixel,
        SamusMissileIgnition.Ignited => (IgnitionSpeed
            ?? throw new InvalidOperationException("An ignited missile keeps the speed it ignited at.")).InTwoHundredFiftySixthsOfPixel,
        _ => throw new InvalidOperationException($"Undefined {nameof(SamusMissileIgnition)} {Ignition}."),
    };
}
