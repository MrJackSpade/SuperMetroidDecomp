using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>What a spawner can spawn (#1392): enemy projectile frames and room effect-actor kinds.</summary>
/// <param name="Projectiles">The projectile frames it can show.</param>
/// <param name="EffectActors">The room effect-actor kinds it can spawn.</param>
internal sealed record SpawnedObjects(IReadOnlyList<EnemyProjectileFrameId> Projectiles, IReadOnlyList<RoomEffectActorKind> EffectActors);
