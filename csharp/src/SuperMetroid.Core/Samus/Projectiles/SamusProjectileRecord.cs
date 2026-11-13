namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One timed projectile record's gameplay (#1301): its animation and place there are the identity installed frame bindings
/// key on, and what it pictures is installed only (#1596).
/// </summary>
/// <param name="Radius">The hitbox's radii.</param>
/// <param name="AnimationFrame">The animation frame a projectile trail reads.</param>
internal readonly record struct SamusProjectileRecord(Extent16<RoomPixels> Radius, int AnimationFrame);
