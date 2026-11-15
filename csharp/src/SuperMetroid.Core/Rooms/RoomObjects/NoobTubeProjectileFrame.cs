using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What one beat of a n00b tube piece's animation shows.</summary>
/// <param name="Shown">The frame the piece shows.</param>
/// <param name="Mirrored">The shard's mirror image across the tube, which a mirroring flicker shows on even updates, if it has one.</param>
internal readonly record struct NoobTubeProjectileFrame(EnemyProjectileFrameId Shown, EnemyProjectileFrameId? Mirrored = null);
