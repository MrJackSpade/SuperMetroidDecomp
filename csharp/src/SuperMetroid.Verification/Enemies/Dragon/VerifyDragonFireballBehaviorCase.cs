using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Dragon;

namespace SuperMetroid.Verification.Enemies.Dragon;

/// <summary>The fireball, rising and falling of one verify dragon fireball behavior case.</summary>
/// <param name="Fireball">The fireball.</param>
/// <param name="Rising">The rising.</param>
/// <param name="Falling">The falling.</param>
internal readonly record struct VerifyDragonFireballBehaviorCase(RoomEnemyProjectileSlot Fireball, DragonFireballProgram Rising, DragonFireballProgram Falling);
