namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What one firing direction adds to each kind of projectile's velocity on every update.</summary>
/// <param name="Beam">
/// A beam's acceleration, which a missile or Super Missile also takes on the update it ignites, before its ignition
/// replaces its velocity.
/// </param>
/// <param name="Missile">An ignited missile's acceleration.</param>
/// <param name="SuperMissile">An ignited Super Missile's acceleration.</param>
internal sealed record SamusDirectionAccelerations(
    EightEightVector<RoomPixels> Beam,
    EightEightVector<RoomPixels> Missile,
    EightEightVector<RoomPixels> SuperMissile);
