using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One timed projectile record (#1301); its index is the identity installed frame bindings key on.</summary>
/// <param name="Sprite">The stock composition the record draws when no binding replaces it.</param>
/// <param name="Radius">The hitbox's radii.</param>
/// <param name="AnimationFrame">The animation frame word a projectile trail reads.</param>
internal readonly record struct SamusProjectileRecord(ProjectileSpriteId Sprite, Extent16<RoomPixels> Radius, int AnimationFrame);
