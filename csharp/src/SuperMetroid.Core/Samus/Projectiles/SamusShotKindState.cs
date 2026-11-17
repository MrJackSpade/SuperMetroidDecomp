namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What one kind of Samus's shots keeps from update to update beyond what every shot keeps (#2346): a missile's ignition
/// (<see cref="MissileShotState"/>), a Super Missile's or its link's (<see cref="SuperMissileShotState"/>), a special
/// attack particle's part and flight (<see cref="SpecialAttackParticleState"/>), a shinespark crash echo's flight
/// (<see cref="ShinesparkEchoShotState"/>) and the Chainsaw shot's fuse (<see cref="ChainsawFuseShotState"/>). A new
/// record is created each time a kind's behaviour is installed on a shot (<see cref="SamusProjectile.Install"/>); the
/// shot keeps it through the later behaviours of its kind and its explosion, until it ends. Beams, the Hyper Beam and
/// explosions keep nothing beyond the shot's own state.
/// </summary>
internal abstract class SamusShotKindState
{
}
