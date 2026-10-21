using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One timed bank-$93 projectile record (#1301).</summary>
/// <param name="Address">The record's bank-$93 address: the identity installed frame bindings key on.</param>
/// <param name="Sprite">The stock composition the record draws when no binding replaces it.</param>
/// <param name="Radius">The hitbox's radii.</param>
/// <param name="AnimationFrame">The animation frame word a projectile trail reads.</param>
internal readonly record struct SamusProjectileRecord(int Address, ProjectileSpriteId Sprite, Extent16<RoomPixels> Radius, int AnimationFrame);
