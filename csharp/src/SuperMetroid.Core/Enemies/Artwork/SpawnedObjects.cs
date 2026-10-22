using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.SpriteObjects;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>What a spawner can spawn (#1392): enemy projectile frames and room sprite-object kinds.</summary>
/// <param name="Projectiles">The projectile frames it can show.</param>
/// <param name="SpriteObjects">The room sprite-object kinds it can spawn.</param>
internal sealed record SpawnedObjects(IReadOnlyList<EnemyProjectileFrameId> Projectiles, IReadOnlyList<RoomSpriteObjectKind> SpriteObjects);
