namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One Samus projectile animation frame's gameplay (#1301): the frame is named by its animation and its place there, the
/// identity installed frame bindings key on, and what it pictures is installed only (#1596).
/// </summary>
/// <param name="Radius">The hitbox's radii.</param>
/// <param name="AnimationFrame">The animation frame a projectile trail reads.</param>
internal readonly record struct SamusProjectileFrameGameplay(Extent16<RoomPixels> Radius, int AnimationFrame);
