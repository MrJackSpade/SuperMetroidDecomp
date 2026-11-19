namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A missile's ignition (#2346), created when the missile is fired or reflected; a Super Missile and its link keep theirs
/// with the link they name (<see cref="SuperMissileShotState"/>).
/// </summary>
internal class MissileShotState : SamusShotKindState
{
    /// <summary>Starts a missile's ignition pending.</summary>
    /// <param name="ignition">Where the missile starts in its ignition: pending when fired, pending after a reflection when reflected.</param>
    /// <exception cref="ArgumentOutOfRangeException">The missile would start ignited.</exception>
    internal MissileShotState(SamusMissileIgnition ignition) =>
        Ignition = Ensure.NotNull(ignition) is SamusMissileIgnition.Pending or SamusMissileIgnition.PendingAfterReflection
            ? ignition
            : throw new ArgumentOutOfRangeException(nameof(ignition), ignition, "A missile starts its ignition pending; it ignites on its update.");

    /// <summary>Where the missile is in its ignition, with the speed it ignited at once it has.</summary>
    internal SamusMissileIgnition Ignition { get; private set; }

    /// <summary>Ignites the missile at <paramref name="speed"/>.</summary>
    /// <param name="speed">The base speed it ignites at: one pixel an update, or more after a reflection's head start.</param>
    internal void Ignite(SignedEightEight speed) => Ignition = new SamusMissileIgnition.Ignited(speed);
}
