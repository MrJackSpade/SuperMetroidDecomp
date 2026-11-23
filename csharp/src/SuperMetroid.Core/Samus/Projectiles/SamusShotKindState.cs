namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What one kind of Samus's shots keeps from update to update beyond what every shot keeps (#2346): a missile's ignition
/// (<see cref="MissileShotState"/>), a Super Missile's or its link's (<see cref="SuperMissileShotState"/>), a special
/// attack particle's part and flight (<see cref="SpecialAttackParticleState"/>) and a shinespark crash echo's flight
/// (<see cref="ShinesparkEchoShotState"/>). A new record is created each time a kind's behaviour is installed on a shot
/// (<see cref="SamusProjectileOccupant.Install{TState}"/>); the projectile holding the place keeps it through the later behaviours of
/// its kind and its explosion, until it ends. Beams, the Hyper Beam and explosions keep nothing beyond the shot's own state, the Power Bomb fuse a beam
/// carries (<see cref="SamusProjectile.BeamFuse"/>) among it.
/// </summary>
internal abstract partial class SamusShotKindState
{
    /// <summary>
    /// Whether the shot is a missile waiting to ignite as it was fired, not after a reflection: a fired Super Missile and
    /// its link, which never ignites, are, and the gold Ninja's armour lets such a Super Missile by.
    /// </summary>
    internal abstract bool AwaitsIgnitionAsFired { get; }

    /// <summary>
    /// The shot place the shot names as its Super Missile link (<see cref="SuperMissileShotState.LinkChainPosition"/>), whose
    /// projectile an enemy's reflection of the shot deletes; none for every other kind and a Super Missile naming none.
    /// </summary>
    internal abstract int? NamedLinkPlace { get; }

    /// <summary>
    /// Whether the shot's own next update deletes it after an enemy's hit, rather than the hit starting its impact: a
    /// special attack's particles do, so the hit only marks the direction they were struck from.
    /// </summary>
    internal abstract bool DeletesItselfAfterEnemyHit { get; }

    /// <summary>What a beam fired into the shot's place finds there (<see cref="BeamFiredOverLastShot"/>).</summary>
    internal abstract BeamFiredOverLastShot.ReplacedShot ReplacedByBeam { get; }
}
