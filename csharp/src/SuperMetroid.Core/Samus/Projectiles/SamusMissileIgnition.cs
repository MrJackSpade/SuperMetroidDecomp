namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Where a missile or Super Missile is in its ignition (#2529). A fired or reflected missile coasts until its first update,
/// then ignites: its velocity is rebuilt from its ignition speed and Samus's inherited motion, and only then do the
/// missile accelerations and a Super Missile's collision link begin. The ignited missile keeps the speed it ignited at.
/// </summary>
public abstract record SamusMissileIgnition
{
    /// <summary>Closes the hierarchy to the stages declared here.</summary>
    private SamusMissileIgnition()
    {
    }

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
    public sealed record Pending : Unignited;

    /// <summary>Reflected by an enemy and not yet re-ignited; it re-ignites with the reflection's head start.</summary>
    public sealed record PendingAfterReflection : Unignited;

    /// <summary>Ignited at <paramref name="Speed"/>.</summary>
    /// <param name="Speed">The base speed it ignited at: <see cref="SamusProjectileMotionDefinitions.MissileIgnitionSpeed"/>, or <see cref="SamusProjectileMotionDefinitions.ReflectedMissileIgnitionSpeed"/> after a reflection's head start.</param>
    public sealed record Ignited(SignedEightEight Speed) : SamusMissileIgnition;
}
