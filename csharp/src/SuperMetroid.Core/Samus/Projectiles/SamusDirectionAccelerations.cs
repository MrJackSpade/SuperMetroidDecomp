namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What one firing direction adds to each kind of projectile's velocity on every update.</summary>
/// <param name="Beam">A beam's acceleration, which an ignited missile also takes before its own.</param>
/// <param name="Missile">An ignited missile's acceleration, after the beam's.</param>
/// <param name="SuperMissile">An ignited Super Missile's acceleration.</param>
internal sealed partial record SamusDirectionAccelerations(
    PixelVelocity<RoomPixels> Beam,
    PixelVelocity<RoomPixels> Missile,
    PixelVelocity<RoomPixels> SuperMissile);
